namespace Oxpecker

[<AutoOpen>]
module ModelValidation =

    open System.Collections.Generic
    open System.ComponentModel.DataAnnotations
    open System.Runtime.CompilerServices
    open Microsoft.AspNetCore.Http

    type ValidationErrors(errors: ResizeArray<ValidationResult>) =
        let errorDict =
            lazy
                (let dict = Dictionary<string, ResizeArray<string|null>>()
                 for error in errors do
                     for memberName in error.MemberNames do
                         match dict.TryGetValue(memberName) with
                         | true, value -> value.Add(error.ErrorMessage)
                         | false, _ ->
                             let arrayList = ResizeArray(1)
                             arrayList.Add(error.ErrorMessage)
                             dict[memberName] <- arrayList
                 dict)
        member this.All: ValidationResult seq = errors
        member this.ErrorMessagesFor(name) : seq<string|null> =
            match errorDict.Value.TryGetValue(name) with
            | true, value -> value
            | false, _ -> Seq.empty

    type InvalidModel<'T> = 'T * ValidationErrors

    [<RequireQualifiedAccess>]
    type ModelState<'T> =
        | Empty
        | Valid of 'T
        | Invalid of InvalidModel<'T>
        member this.Value(f: 'T -> string | null) =
            match this with
            | Empty -> null
            | Valid model -> f model
            | Invalid(model, _) -> f model
        member this.BoolValue(f: 'T -> bool) =
            match this with
            | Empty -> false
            | Valid model -> f model
            | Invalid(model, _) -> f model

    [<RequireQualifiedAccess>]
    type ValidationResult<'T> =
        | Valid of 'T
        | Invalid of InvalidModel<'T>

    let validateModel (model: 'T) =
        let validationResults = ResizeArray()
        match Validator.TryValidateObject(model, ValidationContext(model), validationResults, true) with
        | true -> ValidationResult.Valid model
        | false -> ValidationResult.Invalid(model, ValidationErrors(validationResults))

    type HttpContextExtensions =

        /// <summary>
        /// Uses the <see cref="Serializers.IJsonSerializer"/> to deserialize the entire body of the <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> asynchronously into an object of type 'T and then validate it.
        /// </summary>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
        [<Extension>]
        static member BindAndValidateJson<'T>(ctx: HttpContext) =
            task {
                let! result = ctx.BindJson<'T>()
                return validateModel result
            }

        /// <summary>
        /// Parses all input elements from an HTML form into an object of type 'T and then validates it.
        /// </summary>
        /// <param name="ctx">The current http context object.</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
        [<Extension>]
        static member BindAndValidateForm<'T>(ctx: HttpContext) =
            task {
                let! result = ctx.BindForm<'T>()
                return validateModel result
            }

        /// <summary>
        /// Parses all parameters of a request's query string into an object of type 'T and then validates it.
        /// </summary>
        /// <param name="ctx">The current http context object.</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns an instance of type 'T</returns>
        [<Extension>]
        static member BindAndValidateQuery<'T>(ctx: HttpContext) =
            let result = ctx.BindQuery<'T>()
            validateModel result

    [<AutoOpen>]
    module RequestHandlers =
        /// <summary>
        /// Parses a JSON payload into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type ValidationResult<'T> and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateJson<'T> (f: ValidationResult<'T> -> EndpointHandler) : EndpointHandler =
            fun (ctx: HttpContext) ->
                task {
                    let! model = ctx.BindJson<'T>()
                    return! f (validateModel model) ctx
                }

        /// <summary>
        /// Parses a HTTP form payload into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateForm<'T> (f: ValidationResult<'T> -> EndpointHandler) : EndpointHandler =
            fun (ctx: HttpContext) ->
                task {
                    let! model = ctx.BindForm<'T>()
                    return! f (validateModel model) ctx
                }

        /// <summary>
        /// Parses a HTTP query string into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateQuery<'T> (f: ValidationResult<'T> -> EndpointHandler) : EndpointHandler =
            fun (ctx: HttpContext) ->
                let model = ctx.BindQuery<'T>()
                f (validateModel model) ctx
