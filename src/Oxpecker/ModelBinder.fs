namespace Oxpecker

open System
open System.Collections.Generic
open System.Globalization
open Microsoft.Extensions.Primitives

/// <summary>
/// Interface defining Form and Query parsing methods.
/// Use this interface to customize Form and Query parsing in Oxpecker.
/// </summary>
type IModelBinder =
    abstract member Bind<'T> : seq<KeyValuePair<string, StringValues>> -> 'T

/// <summary>
/// Module for parsing models from a generic data set.
/// </summary>
module internal ModelParser =

    open System.Reflection
    open System.ComponentModel
    open System.Text.RegularExpressions
    open Microsoft.FSharp.Reflection
    open Microsoft.FSharp.Collections

    let listGenericType = typedefof<List<_>>
    let optionGenericType = typedefof<Option<_>>
    let nullableGenericType = typedefof<Nullable<_>>

    type private Type with
        member this.IsGeneric() = this.GetTypeInfo().IsGenericType

        member this.IsFSharpList() =
            match this.IsGeneric() with
            | false -> false
            | true -> this.GetGenericTypeDefinition() = listGenericType

        member this.IsFSharpOption() =
            match this.IsGeneric() with
            | false -> false
            | true -> this.GetGenericTypeDefinition() = optionGenericType

        member this.GetGenericType() = this.GetGenericArguments().[0]

        member this.MakeNoneCase() =
            let cases = FSharpType.GetUnionCases this
            FSharpValue.MakeUnion(cases[0], [||])

        member this.MakeSomeCase(value: obj) =
            let cases = FSharpType.GetUnionCases this
            FSharpValue.MakeUnion(cases[1], [| value |])

    /// Returns either a successfully parsed object `'T` or a `string` error message containing the parsing error.
    let rec private parseValue (t: Type) (rawValues: StringValues) (culture: CultureInfo) : Result<obj | null, string> =

        // First establish some basic type information:
        let isGeneric = t.IsGeneric()
        let isList, isOption, genArgType =
            match isGeneric with
            | true -> t.IsFSharpList(), t.IsFSharpOption(), t.GetGenericType()
            | false -> false, false, Unchecked.defaultof<Type>

        if t.IsArray then
            let arrArgType = t.GetElementType() |> Unchecked.nonNull
            let arrLen = rawValues.Count
            let arr = Array.CreateInstance(arrArgType, arrLen)
            if arrLen = 0 then
                Ok(box arr)
            else
                let items, _, error =
                    Array.fold
                        (fun (items: Array, idx: int, error: string option) (rawValue: string | null) ->
                            let nIdx = idx + 1
                            match error with
                            | Some _ -> arr, nIdx, error
                            | None ->
                                match parseValue arrArgType (StringValues rawValue) culture with
                                | Error err -> arr, nIdx, Some err
                                | Ok item ->
                                    items.SetValue(item, idx)
                                    items, nIdx, None)
                        (arr, 0, None)
                        (rawValues.ToArray())
                match error with
                | Some err -> Error err
                | None -> Ok(box items)
        elif isList then
            let cases = FSharpType.GetUnionCases t
            let emptyList = FSharpValue.MakeUnion(cases[0], [||])
            if rawValues.Count = 0 then
                Ok emptyList
            else
                let consCase = cases[1]
                let items, error =
                    Array.foldBack
                        (fun (rawValue: string | null) (items: obj | null, error: string option) ->
                            match error with
                            | Some _ -> emptyList, error
                            | None ->
                                match parseValue genArgType (StringValues rawValue) culture with
                                | Error err -> emptyList, Some err
                                | Ok item -> FSharpValue.MakeUnion(consCase, [| item; items |]), None)
                        (rawValues.ToArray())
                        (emptyList, None)
                match error with
                | Some err -> Error err
                | None -> Ok items
        elif isGeneric then
            let result = parseValue genArgType rawValues culture
            match result with
            | Error err -> Error err
            | Ok value ->
                match isOption with
                | false -> Ok value
                | true ->
                    match isNull value with
                    | true -> t.MakeNoneCase()
                    | false -> t.MakeSomeCase(value)
                    |> Ok
        elif FSharpType.IsUnion t then
            let unionName = rawValues.ToString()
            let cases = FSharpType.GetUnionCases t
            if String.IsNullOrWhiteSpace unionName then
                Error $"Cannot parse an empty value to type %s{t.ToString()}."
            else
                cases
                |> Array.tryFind(_.Name.Equals(unionName, StringComparison.OrdinalIgnoreCase))
                |> function
                    | Some case -> Ok(FSharpValue.MakeUnion(case, [||]))
                    | None -> Error $"The value '%s{unionName}' is not a valid case for type %s{t.ToString()}."
        else
            let converter =
                if t.GetTypeInfo().IsValueType then
                    nullableGenericType.MakeGenericType([| t |])
                else
                    t
                |> TypeDescriptor.GetConverter
            let rawValue = rawValues.ToString()
            try
                converter.ConvertFromString(null, culture, rawValue) |> Ok
            with _ ->
                $"Could not parse value '%s{rawValue}' to type %s{t.ToString()}." |> Error


    [<RequireQualifiedAccess>]
    type internal ParseResult =
        | Success of obj
        | Error of string
        | Skip

    let rec internal parseModel<'T>
        (model: 'T)
        (culture: CultureInfo)
        (data: IDictionary<string, StringValues>)
        : Result<'T, string> =
        // Normalize data
        let normalizeKey (key: string) = key.TrimEnd([| '['; ']' |])
        let data = data |> Seq.map(fun i -> normalizeKey i.Key, i.Value) |> dict

        let error =
            // Iterate through all properties of the model
            model.GetType().GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
            |> Seq.filter _.CanWrite
            |> Seq.fold
                (fun (error: string option) (prop: PropertyInfo) ->
                    // If previous property failed to parse then short circuit the parsing and return the error.
                    if error.IsSome then
                        error
                    else
                        let parsingResult =
                            // Check the provided dictionary for an entry which matches the
                            // current property name. If no entry can be found, then try to
                            // generate a value without any data (will only work for an option type).
                            // If there was an entry then try to parse the raw value.
                            match data.TryGetValue(prop.Name) with
                            | false, _ ->
                                match getValueForArrayOfGenericType culture data prop with
                                | Some v -> ParseResult.Success v
                                | None ->
                                    match getValueForComplexType culture data prop with
                                    | Some v -> ParseResult.Success v
                                    | None ->
                                        match getValueForMissingProperty prop.PropertyType with
                                        | Some v -> ParseResult.Success v
                                        | None -> ParseResult.Skip
                            | true, rawValue ->
                                match parseValue prop.PropertyType rawValue culture with
                                | Ok v -> ParseResult.Success v
                                | Error e -> ParseResult.Error e

                        // Check if a value was able to get successfully parsed.
                        // If there was an error then return the error.
                        // If no corresponding data was found then skip setting a value
                        // but don't return an error so that the parsing of other properties can continue.
                        // If a value was successfully parsed, then set the value on the property of the model.
                        match parsingResult with
                        | ParseResult.Error err -> Some err
                        | ParseResult.Skip -> None
                        | ParseResult.Success value ->
                            prop.SetValue(model, value, null)
                            None)
                None
        // Only return the model if all properties were successfully
        match error with
        | Some err -> Error err
        | _ -> Ok model

    /// Returns a value (the None union case) if the type is `Option<'T>` otherwise `None`.
    and getValueForMissingProperty (t: Type) =
        match t.IsFSharpOption() with
        | false -> None
        | true -> Some(t.MakeNoneCase())

    and getValueForComplexType
        (culture: CultureInfo)
        (data: IDictionary<string, StringValues>)
        (prop: PropertyInfo)
        : obj option =
        let isMaybeComplexType = data.Keys |> Seq.exists(_.StartsWith(prop.Name + "."))
        let isRecordType = FSharpType.IsRecord prop.PropertyType
        let isGenericType = prop.PropertyType.IsGenericType
        let tryResolveComplexType = isMaybeComplexType && (isRecordType || isGenericType)

        if tryResolveComplexType then
            let regex = prop.Name |> Regex.Escape |> sprintf @"%s\.(\w+)" |> Regex

            let dictData =
                data
                |> Seq.filter(fun item -> regex.IsMatch item.Key)
                |> Seq.map(fun item ->
                    let matchedData = regex.Match item.Key
                    let key = matchedData.Groups[1].Value
                    let value = item.Value
                    key, value)
                |> Seq.fold (fun (state: Map<string, StringValues>) -> state.Add) Map.empty

            match prop.PropertyType.IsFSharpOption() with
            | false ->
                let model = Activator.CreateInstance(prop.PropertyType)
                let res = parseModel model culture dictData
                match res with
                | Ok o -> o |> Option.ofObj
                | Error _ -> None
            | true ->
                let genericType = prop.PropertyType.GetGenericType()
                let model = Activator.CreateInstance(genericType)
                let res = parseModel model culture dictData
                match res with
                | Ok o -> prop.PropertyType.MakeSomeCase(o) |> Option.ofObj
                | Error _ -> None
        else
            None

    and getValueForArrayOfGenericType
        (culture: CultureInfo)
        (data: IDictionary<string, StringValues>)
        (prop: PropertyInfo)
        : obj option =

        if prop.PropertyType.IsArray then
            let regex = prop.Name |> Regex.Escape |> sprintf @"%s\[(\d+)\]\.(\w+)" |> Regex

            let innerType = prop.PropertyType.GetElementType()

            let arrOfValues =
                data
                |> Seq.filter(fun item -> regex.IsMatch item.Key)
                |> Seq.map(fun item ->
                    let matchedData = regex.Match item.Key
                    let index = matchedData.Groups[1].Value
                    let key = matchedData.Groups[2].Value
                    let value = item.Value
                    index, key, value)
                |> Seq.groupBy(fun (index, _, _) -> index |> int)
                |> Seq.sortBy fst
                |> Seq.choose(fun (index, values) ->
                    let dictData =
                        values
                        |> Seq.fold
                            (fun (state: Map<string, StringValues>) (_, key, value) -> state.Add(key, value))
                            Map.empty

                    let model = Activator.CreateInstance(innerType)
                    let res = parseModel model culture dictData

                    match res with
                    | Ok o -> Some(index, o)
                    | Error _ -> None)
                |> Seq.toArray

            let arrayOfObjects =
                if (arrOfValues |> Array.length > 0) then
                    let arraySize = (arrOfValues |> Array.last |> fst) + 1
                    let arrayOfObjects = Array.CreateInstance(innerType, arraySize)

                    arrOfValues
                    |> Array.iter(fun (index, item) -> arrayOfObjects.SetValue(item, index))

                    arrayOfObjects
                else
                    Array.CreateInstance(innerType, 0)

            arrayOfObjects |> box |> Option.ofObj
        else
            None


/// <summary>
/// Configuration options for the default <see cref="Oxpecker.ModelBinder"/>
/// </summary>
type ModelBinderOptions = {
    CultureInfo: CultureInfo
} with
    static member Default = {
        CultureInfo = CultureInfo.InvariantCulture
    }

/// Default implementation of the <see cref="Oxpecker.IModelBinder"/>
type ModelBinder(?options: ModelBinderOptions) =
    let options = defaultArg options <| ModelBinderOptions.Default

    interface IModelBinder with
        /// <summary>
        /// Tries to create an instance of type 'T from a given set of data.
        /// It will try to match each property of 'T with a key from the data dictionary and parse the associated value to the value of 'T's property.
        /// </summary>
        member this.Bind<'T>(data) =
            let model = Activator.CreateInstance<'T>()
            match ModelParser.parseModel<'T> model options.CultureInfo (Dictionary data) with
            | Ok value -> value
            | Error msg -> failwith msg
