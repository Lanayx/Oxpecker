namespace Oxpecker.Solid.FablePlugin

// Contains logic to enable JSON conversion and printing of AST to assist
// in plugin development and debugging.
//
// The intention is to provide JSON that can be viewed in a tree structure with any
// compatible JSON viewing tool
//
// There are several CommandLine defines that can be provided to enable and
// modify this behaviour.
// The printing of the AST in JSON has a depth limit to provide the relevant depth
// of information on nodes that are being transformed.
// Otherwise, you would want to redirect your console output to a file.
// The tracers, when enabled, emit on most transformations and transitions in the plugin logic,
// so the output very quickly can become verbose
//
// =======
// DEFINES
// =======
// OXPECKER_SOLID_MINIMAL    //* No Json printing, just operation flow is printed
// OXPECKER_SOLID_DEBUG      //* Max depth of 4 - default
// OXPECKER_SOLID_DEBUG_1    //* Max depth of 1
// ''  ''    ''  ''     [n]  //* Max depth of n
// OXPECKER_SOLID_TRACE      //* No max depth.
//
// OXPECKER_SOLID_MINIMAL                               //* This combo will cause the json to be slim.
// && (OXPECKER_SOLID_DEBUG || OXPECKER_SOLID_TRACE)    //  ie we replace things like paths with nulls
//
// =======
// USAGE
// =======
// fable --define OXPECKER_SOLID_DEBUG

open System
open Fable.AST
open Fable.AST.Fable
open Types
open System.Text.Json

[<AutoOpen>]
module private CompilerDirectives =
    // Converters can use a simple one liner at the beginning of their Write to see if
    // 1) they can write (if some debug mode is on; although this wouldnt be reached in that case
    // 2) the max depth has been reached as per some other definition or directive
    let private maxDepth: unit -> int = PluginConfiguration.Depth
    let mutable private _canWrite = fun (writer: Utf8JsonWriter) -> false
    _canWrite <-
        fun writer ->
            if maxDepth() > 0 && writer.CurrentDepth > maxDepth() then
                writer.WriteRawValue($"\"MAX_DEPTH\"", true)
                false
            else
                true
    let canWrite = _canWrite
    let simplify = PluginConfiguration.Slim
module private rec PrettyPrint =
    (*  The Converters are all written the same.
    The Write logic is wrapped in an `if` statement. It checks against the current depth
    of the writer with whatever Compiler Directives are enabled to determine if it should
    proceed. If not, it writes "MAX_DEPTH".

  * We only have to provide converters for the Discriminated Unions (DUs).
    Whenever writing JSON output, you should assume you are writing in the 'value' position.
    ie. Wrap all non-value output in an object or array.

  * Remember the output is not supposed to be decodable, or accurately reflect the underlying
    types.

  * DUs should have their name embedded in the object value. The PropertyName should be the
    DU type wrapped in __, and the value would be the DU Value.
    GOOD_OUTPUT: {"__EntityPath__":"SourcePath",...:...}

  * Prepend all DUs choices with the root type name.
    BAD_OUTPUT: {"SourcePath":"value"}
    GOOD_OUTPUT: {"EntityPath.SourcePath":"value"}

  * If DU tuples are named, it is valuable information on the context of the values.
    Why wrap them in an array?
    Wrap them in an anonymous object (with the fields named according to the tuple) and use
    JsonSerializer. Or, write the object with the field/values set by hand.
    Obviously simple/single value/non-tupled DUs don't have to do this.
    BAD_OUTPUT: {"Constraint.HasMember":[{...}, false]}
    GOOD_OUTPUT: {"Constraint.HasMember":{"name":{...}, "isStatic":false}}                      *)
    let wrapUnionType str = $"__{str}__"
    type EntityPathConverter() =
        inherit Serialization.JsonConverter<EntityPath>()
        override this.CanConvert typ = typ = typeof<EntityPath>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if simplify() then
                nameof EntityPath |> wrapUnionType |> writer.WriteStringValue
            elif  canWrite writer then
                let prefix postfix = $"{nameof EntityPath}.{postfix}"
                writer.WriteStartObject()
                match value with
                | SourcePath s -> writer.WriteString(prefix(nameof SourcePath), s)
                | AssemblyPath s -> writer.WriteString(prefix(nameof AssemblyPath), s)
                | CoreAssemblyName s -> writer.WriteString(prefix(nameof CoreAssemblyName), s)
                | PrecompiledLib(sourcePath, assemblyPath) ->
                    writer.WriteString(
                        prefix(nameof PrecompiledLib),
                        JsonSerializer.Serialize {|
                            sourcePath = sourcePath
                            assemblyPath = assemblyPath
                        |}
                    )
                writer.WriteEndObject()
    type MemberRefConverter() =
        inherit Serialization.JsonConverter<MemberRef>()
        override this.CanConvert typ = typ = typeof<MemberRef>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof MemberRef}.{postfix}"
                writer.WriteStartObject()
                match value with
                | MemberRef(declaringEntity, info) ->
                    let inp = {|
                        declaringEntity = declaringEntity
                        info = info
                    |}
                    writer.WritePropertyName(prefix(nameof MemberRef))
                    JsonSerializer.Serialize(writer, inp, inp.GetType(), options)
                | GeneratedMemberRef generatedMember ->
                    nameof GeneratedMemberRef |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, generatedMember, generatedMember.GetType(), options)
                writer.WriteEndObject()
    type GeneratedMemberConverter() =
        inherit Serialization.JsonConverter<GeneratedMember>()
        override this.CanConvert typ = typ = typeof<GeneratedMember>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof GeneratedMember}.{postfix}"
                match value with
                | GeneratedFunction info ->
                    let object = {| GeneratedFunction = info |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | GeneratedValue info ->
                    let object = {| GeneratedValue = info |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | GeneratedGetter info ->
                    let object = {| GeneratedGetter = info |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | GeneratedSetter info ->
                    let object = {| GeneratedSetter = info |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
    type NumberInfoConverter() =
        inherit Serialization.JsonConverter<NumberInfo>()
        override this.CanConvert typ = typ = typeof<NumberInfo>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof NumberInfo}.{postfix}"
                writer.WriteStartObject()
                match value with
                | NumberInfo.Empty -> nameof NumberInfo.Empty |> prefix |> writer.WriteNull
                | NumberInfo.IsMeasure fullname ->
                    nameof NumberInfo.IsMeasure
                    |> prefix
                    |> fun prop -> writer.WriteString(prop, fullname)
                | NumberInfo.IsEnum ent ->
                    nameof NumberInfo.IsEnum |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, ent, ent.GetType(), options)
                writer.WriteEndObject()
    type NumberValueConverter() =
        inherit Serialization.JsonConverter<NumberValue>()
        override this.CanConvert typ = typ = typeof<NumberValue>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                match value with
                | NumberValue.Int8 b -> JsonSerializer.Serialize(writer, b, typeof<sbyte>, options)
                | NumberValue.UInt8 b -> JsonSerializer.Serialize(writer, b, typeof<byte>, options)
                | NumberValue.Int16 s -> JsonSerializer.Serialize(writer, s, typeof<int16>, options)
                | NumberValue.UInt16 s -> JsonSerializer.Serialize(writer, s, typeof<uint16>, options)
                | NumberValue.Int32 i -> JsonSerializer.Serialize(writer, i, typeof<int32>, options)
                | NumberValue.UInt32 i -> JsonSerializer.Serialize(writer, i, typeof<uint32>, options)
                | NumberValue.Int64 int64 -> JsonSerializer.Serialize(writer, int64, int64.GetType(), options)
                | NumberValue.UInt64 uInt64 -> JsonSerializer.Serialize(writer, uInt64, uInt64.GetType(), options)
                | NumberValue.Int128(upper, lower) -> $"{upper}{lower}" |> fun v -> writer.WriteRawValue(v, true)
                | NumberValue.UInt128(upper, lower) -> $"{upper}{lower}" |> fun v -> writer.WriteRawValue(v, true)
                | NumberValue.BigInt bigInteger ->
                    JsonSerializer.Serialize(writer, bigInteger, bigInteger.GetType(), options)
                | NumberValue.NativeInt intPtr -> JsonSerializer.Serialize(writer, intPtr, intPtr.GetType(), options)
                | NumberValue.UNativeInt uIntPtr ->
                    JsonSerializer.Serialize(writer, uIntPtr, uIntPtr.GetType(), options)
                | NumberValue.Float16 f -> JsonSerializer.Serialize(writer, f, f.GetType(), options)
                | NumberValue.Float32 f -> JsonSerializer.Serialize(writer, f, f.GetType(), options)
                | NumberValue.Float64 f -> JsonSerializer.Serialize(writer, f, f.GetType(), options)
                | NumberValue.Decimal decimal -> JsonSerializer.Serialize(writer, decimal, decimal.GetType(), options)
    type ArrayKindConverter() =
        inherit Serialization.JsonConverter<ArrayKind>()
        override this.CanConvert typ = typ = typeof<ArrayKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteStartObject()
                match value with
                | ResizeArray -> nameof ResizeArray
                | MutableArray -> nameof MutableArray
                | ImmutableArray -> nameof ImmutableArray
                |> fun value -> writer.WriteString(nameof ArrayKind, value)
                writer.WriteEndObject()
    type ConstraintConverter() =
        inherit Serialization.JsonConverter<Constraint>()
        override this.CanConvert typ = typ = typeof<Constraint>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let wrap input = $"\"{input}\""
                writer.WriteStartObject()
                match value with
                | Constraint.HasMember(name, isStatic) ->
                    writer.WritePropertyName $"{nameof Constraint}.{nameof Constraint.HasMember}"
                    let object = {| name = name; isStatic = isStatic |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Constraint.CoercesTo target ->
                    writer.WritePropertyName $"{nameof Constraint}.{nameof Constraint.CoercesTo}"
                    JsonSerializer.Serialize(writer, target, typeof<Fable.Type>, options)
                | Constraint.IsNullable ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.IsNullable}"
                    writer.WriteRawValue(object, true)
                | Constraint.IsValueType ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.IsValueType}"
                    writer.WriteRawValue(object, true)
                | Constraint.IsReferenceType ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.IsReferenceType}"
                    writer.WriteRawValue(object, true)
                | Constraint.HasDefaultConstructor ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.HasDefaultConstructor}"
                    writer.WriteRawValue(object, true)
                | Constraint.HasComparison ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.HasComparison}"
                    writer.WriteRawValue(object, true)
                | Constraint.HasEquality ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.HasEquality}"
                    writer.WriteRawValue(object, true)
                | Constraint.IsUnmanaged ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.IsUnmanaged}"
                    writer.WriteRawValue(object, true)
                | Constraint.IsEnum ->
                    let object = wrap $"{nameof Constraint}.{nameof Constraint.IsEnum}"
                    writer.WriteRawValue(object, true)
                writer.WriteEndObject()
    type TypeConverter() =
        inherit Serialization.JsonConverter<Fable.Type>()
        override this.CanConvert typ = typ = typeof<Fable.Type>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let wrap input = $"\"{input}\""
                let prefix postfix = $"{nameof Fable.Type}.{postfix}"
                match value with
                | Type.Measure fullname -> writer.WriteString(prefix(nameof Measure), fullname)
                | Type.MetaType ->
                    let object = (nameof MetaType) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.Any -> let object = (nameof Any) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.Unit -> let object = (nameof Unit) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.Boolean -> let object = (nameof Boolean) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.Char -> let object = (nameof Char) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.String -> let object = (nameof String) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.Regex -> let object = (nameof Regex) |> prefix |> wrap in writer.WriteRawValue(object, true)
                | Type.Number(kind, info) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof Number))
                    let object = {| kind = kind; info = info |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.Option(genericArg, isStruct) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof Option))
                    let object = {|
                        genericArg = genericArg
                        isStruct = isStruct
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.Tuple(genericArgs, isStruct) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof Tuple))
                    let object = {|
                        genericArgs = genericArgs
                        isStruct = isStruct
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.Array(genericArg, kind) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof Array))
                    let object = {|
                        genericArg = genericArg
                        kind = kind
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.List genericArg ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof List))
                    let object = {| genericArg = genericArg |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.LambdaType(argType, returnType) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof LambdaType))
                    let object = {|
                        argType = argType
                        returnType = returnType
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.DelegateType(argTypes, returnType) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof DelegateType))
                    let object = {|
                        argTypes = argTypes
                        returnType = returnType
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.GenericParam(name, isMeasure, constraints) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof GenericParam))
                    let object = {|
                        name = name
                        isMeasure = isMeasure
                        constraints = constraints
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.DeclaredType(rref, genericArgs) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof DeclaredType))
                    let object = {|
                        rref = rref
                        genericArgs = genericArgs
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
                | Type.AnonymousRecordType(fieldNames, genericArgs, isStruct) ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(prefix(nameof AnonymousRecordType))
                    let object = {|
                        fieldNames = fieldNames
                        genericArgs = genericArgs
                        isStruct = isStruct
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                    writer.WriteEndObject()
    type ExprConverter() =
        inherit Serialization.JsonConverter<Expr>()
        override this.CanConvert typ = typ = typeof<Expr>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof Expr}.{postfix}"
                writer.WriteStartObject()
                match value with
                | IdentExpr ident ->
                    writer.WritePropertyName(prefix(nameof IdentExpr))
                    JsonSerializer.Serialize(writer, ident, ident.GetType(), options)
                | Value(kind, range) ->
                    writer.WritePropertyName(prefix(nameof Value))
                    let object = {| kind = kind; range = range |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Lambda(arg, body, name) ->
                    writer.WritePropertyName(prefix(nameof Lambda))
                    let object = {|
                        arg = arg
                        body = body
                        name = name
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Delegate(args, body, name, tags) ->
                    writer.WritePropertyName(prefix(nameof Delegate))
                    let object = {|
                        args = args
                        body = body
                        name = name
                        tags = tags
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | ObjectExpr(members, typ, baseCall) ->
                    writer.WritePropertyName(prefix(nameof ObjectExpr))
                    let object = {|
                        members = members
                        ``type`` = typ
                        baseCall = baseCall
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | TypeCast(expr, typ) ->
                    writer.WritePropertyName(prefix(nameof TypeCast))
                    let object = {| expr = expr; ``type`` = typ |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Test(expr, kind, range) ->
                    writer.WritePropertyName(prefix(nameof Test))
                    let object = {|
                        expr = expr
                        kind = kind
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Call(callee, info, typ, range) ->
                    writer.WritePropertyName(prefix(nameof Call))
                    let object = {|
                        callee = callee
                        info = info
                        ``type`` = typ
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | CurriedApply(applied, args, typ, range) ->
                    writer.WritePropertyName(prefix(nameof CurriedApply))
                    let object = {|
                        applied = applied
                        args = args
                        ``type`` = typ
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Operation(kind, tags, typ, range) ->
                    writer.WritePropertyName(prefix(nameof Operation))
                    let object = {|
                        kind = kind
                        tags = tags
                        ``type`` = typ
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Import(info, typ, range) ->
                    writer.WritePropertyName(prefix(nameof Import))
                    let object = {| info = info; range = range |}
                    writer.WriteStartObject()
                    writer.WritePropertyName "info"
                    JsonSerializer.Serialize(writer, info, typeof<ImportInfo>, options)
                    writer.WritePropertyName "type"
                    JsonSerializer.Serialize(writer, typ, typeof<Fable.Type>, options)
                    writer.WritePropertyName "range"
                    JsonSerializer.Serialize(writer, range, typeof<SourceLocation option>, options)
                    writer.WriteEndObject()
                | Emit(info, typ, range) ->
                    writer.WritePropertyName(prefix(nameof Emit))
                    let object = {|
                        info = info
                        ``type`` = typ
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | DecisionTree(expr, targets) ->
                    writer.WritePropertyName(prefix(nameof DecisionTree))
                    let object = {| expr = expr; targets = targets |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | DecisionTreeSuccess(targetIndex, boundValues, typ) ->
                    writer.WritePropertyName(prefix(nameof DecisionTreeSuccess))
                    let object = {|
                        targetIndex = targetIndex
                        boundValues = boundValues
                        ``type`` = typ
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Let(ident, value, body) ->
                    writer.WritePropertyName(prefix(nameof Let))
                    let object = {|
                        ident = ident
                        value = value
                        body = body
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | LetRec(bindings, body) ->
                    writer.WritePropertyName(prefix(nameof LetRec))
                    let object = {| bindings = bindings; body = body |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Get(expr, kind, typ, range) ->
                    writer.WritePropertyName(prefix(nameof Get))
                    let object = {|
                        expr = expr
                        kind = kind
                        ``type`` = typ
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Set(expr, kind, typ, value, range) ->
                    writer.WritePropertyName(prefix(nameof Set))
                    let object = {|
                        expr = expr
                        kind = kind
                        ``type`` = typ
                        value = value
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Sequential exprs ->
                    writer.WritePropertyName(prefix(nameof Sequential))
                    let object = exprs
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | WhileLoop(guard, body, range) ->
                    writer.WritePropertyName(prefix(nameof WhileLoop))
                    let object = {|
                        guard = guard
                        body = body
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | ForLoop(ident, start, limit, body, isUp, range) ->
                    writer.WritePropertyName(prefix(nameof ForLoop))
                    let object = {|
                        ident = ident
                        start = start
                        limit = limit
                        body = body
                        isUp = isUp
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | TryCatch(body, catch, finalizer, range) ->
                    writer.WritePropertyName(prefix(nameof TryCatch))
                    let object = {|
                        body = body
                        catch = catch
                        finalizer = finalizer
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
                    writer.WritePropertyName(prefix(nameof IfThenElse))
                    let object = {|
                        guardExpr = guardExpr
                        thenExpr = thenExpr
                        elseExpr = elseExpr
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Unresolved(expr, typ, range) ->
                    writer.WritePropertyName(prefix(nameof Unresolved))
                    let object = {|
                        expr = expr
                        ``type`` = typ
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Extended(expr, range) ->
                    writer.WritePropertyName(prefix(nameof Extended))
                    let object = {| expr = expr; range = range |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                writer.WriteEndObject()
    type ValueKindConverter() =
        inherit Serialization.JsonConverter<ValueKind>()
        override this.CanConvert typ = typ = typeof<ValueKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof ValueKind}.{postfix}"
                let wrap input = $"\"{input}\""
                writer.WriteStartObject()
                match value with
                | ThisValue typ ->
                    nameof ThisValue |> prefix |> wrap |> writer.WritePropertyName
                    let object = typ
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | BaseValue(boundIdent, typ) ->
                    nameof BaseValue |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        boundIdent = boundIdent
                        ``type`` = typ
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | TypeInfo(typ, tags) ->
                    nameof TypeInfo |> prefix |> wrap |> writer.WritePropertyName
                    let object = {| ``type`` = typ; tags = tags |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Null typ ->
                    nameof Null |> prefix |> wrap |> writer.WritePropertyName
                    let object = typ
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | UnitConstant ->
                    nameof UnitConstant |> prefix |> wrap |> writer.WritePropertyName
                    let object = ()
                    JsonSerializer.Serialize(writer, object, typeof<unit>, options)
                | BoolConstant value ->
                    nameof BoolConstant |> prefix |> wrap |> writer.WritePropertyName
                    let object = value
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | CharConstant value ->
                    nameof CharConstant |> prefix |> wrap |> writer.WritePropertyName
                    let object = value
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | StringConstant value ->
                    nameof StringConstant |> prefix |> wrap |> writer.WritePropertyName
                    let object = value
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | StringTemplate(tag, parts, values) ->
                    nameof StringTemplate |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        tag = tag
                        parts = parts
                        values = values
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NumberConstant(value, info) ->
                    nameof NumberConstant |> prefix |> wrap |> writer.WritePropertyName
                    let object = {| value = value; info = info |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | RegexConstant(source, flags) ->
                    nameof RegexConstant |> prefix |> wrap |> writer.WritePropertyName
                    let object = {| source = source; flags = flags |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewOption(value, typ, isStruct) ->
                    nameof NewOption |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        value = value
                        ``type`` = typ
                        isStruct = isStruct
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewArray(newKind, typ, kind) ->
                    nameof NewArray |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        newKind = newKind
                        ``type`` = typ
                        kind = kind
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewList(headAndTail, typ) ->
                    nameof NewList |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        headAndTail = headAndTail
                        ``type`` = typ
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewTuple(values, isStruct) ->
                    nameof NewTuple |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        values = values
                        isStruct = isStruct
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewRecord(values, rref, genArgs) ->
                    nameof NewRecord |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        values = values
                        rref = rref
                        genArgs = genArgs
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewAnonymousRecord(values, fieldNames, genArgs, isStruct) ->
                    nameof NewAnonymousRecord |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        values = values
                        fieldNames = fieldNames
                        genArgs = genArgs
                        isStruct = isStruct
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NewUnion(values, tag, rref, genArgs) ->
                    nameof NewUnion |> prefix |> wrap |> writer.WritePropertyName
                    let object = {|
                        values = values
                        tag = tag
                        rref = rref
                        genArgs = genArgs
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                writer.WriteEndObject()
    type TagSourceConverter() =
        inherit Serialization.JsonConverter<TagSource>()
        override this.CanConvert typ = typ = typeof<TagSource>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof TagSource}.{postfix}"
                writer.WriteStartObject()
                match value with
                | AutoImport tagName -> writer.WriteString(prefix(nameof AutoImport), tagName)
                | LibraryImport imp ->
                    writer.WritePropertyName(prefix(nameof LibraryImport))
                    JsonSerializer.Serialize(writer, imp, typeof<Expr>, options)
                writer.WriteEndObject()
    type TagInfoConverter() =
        inherit Serialization.JsonConverter<TagInfo>()
        override this.CanConvert typ = typ = typeof<TagInfo>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof TagInfo}.{postfix}"
                writer.WriteStartObject()
                match value with
                | WithChildren(tagName, propsAndChildren, range) ->
                    nameof WithChildren |> prefix |> writer.WritePropertyName
                    let object = {|
                        tagName = tagName
                        propsAndChildren = propsAndChildren
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | NoChildren(tagName, props, range) ->
                    nameof NoChildren |> prefix |> writer.WritePropertyName
                    let object = {|
                        tagName = tagName
                        props = props
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                | Combined(tagName, props, propsAndChildren, range) ->
                    nameof Combined |> prefix |> writer.WritePropertyName
                    let object = {|
                        tagName = tagName
                        props = props
                        propsAndChildren = propsAndChildren
                        range = range
                    |}
                    JsonSerializer.Serialize(writer, object, object.GetType(), options)
                writer.WriteEndObject()
    type ImportKindConverter() =
        inherit Serialization.JsonConverter<ImportKind>()
        override this.CanConvert typ = typ = typeof<ImportKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof ImportKind}.{postfix}"
                let wrap input = $"\"{input}\""
                match value with
                | UserImport isInline -> nameof UserImport |> prefix |> wrap |> (fun v -> writer.WriteRawValue(v, true))
                | ImportKind.LibraryImport info ->
                    writer.WriteStartObject()
                    nameof ImportKind.LibraryImport |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, info, typeof<LibraryImportInfo>, options)
                    writer.WriteEndObject()
                | MemberImport memberRef ->
                    writer.WriteStartObject()
                    nameof MemberImport |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, memberRef, typeof<MemberRef>, options)
                    writer.WriteEndObject()
                | ClassImport entRef ->
                    writer.WriteStartObject()
                    nameof ClassImport |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, entRef, typeof<EntityRef>, options)
                    writer.WriteEndObject()
    type DeclarationConverter() =
        inherit Serialization.JsonConverter<Declaration>()
        override this.CanConvert typ = typ = typeof<Declaration>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof Declaration}.{postfix}"
                let wrap input = $"\"{input}\""
                writer.WriteStartObject()
                match value with
                | ModuleDeclaration moduleDecl ->
                    nameof ModuleDeclaration |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, moduleDecl, typeof<ModuleDecl>, options)
                | ActionDeclaration actionDecl ->
                    nameof ActionDeclaration |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, actionDecl, typeof<ActionDecl>, options)
                | MemberDeclaration memberDecl ->
                    nameof MemberDeclaration |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, memberDecl, typeof<MemberDecl>, options)
                | ClassDeclaration classDecl ->
                    nameof ClassDeclaration |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, classDecl, typeof<ClassDecl>, options)
                writer.WriteEndObject()
    type NewArrayKindConverter() =
        inherit Serialization.JsonConverter<NewArrayKind>()
        override this.CanConvert typ = typ = typeof<NewArrayKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof NewArrayKind}.{postfix}"
                let wrap input = $"\"{input}\""
                writer.WriteStartObject()
                match value with
                | ArrayValues values ->
                    nameof ArrayValues |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, values, typeof<Expr list>, options)
                | ArrayAlloc size ->
                    nameof ArrayAlloc |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, size, typeof<Expr>, options)
                | ArrayFrom expr ->
                    nameof ArrayFrom |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, expr, typeof<Expr>, options)
                writer.WriteEndObject()
    type GetKindConverter() =
        inherit Serialization.JsonConverter<GetKind>()
        override this.CanConvert typ = typ = typeof<GetKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof GetKind}.{postfix}"
                let wrap input = $"\"{input}\""
                match value with
                | TupleIndex index ->
                    writer.WriteStartObject()
                    nameof TupleIndex |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, index, typeof<int>, options)
                    writer.WriteEndObject()
                | ExprGet expr ->
                    writer.WriteStartObject()
                    nameof ExprGet |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, expr, typeof<Expr>, options)
                    writer.WriteEndObject()
                | FieldGet info ->
                    writer.WriteStartObject()
                    nameof FieldGet |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, info, typeof<FieldInfo>, options)
                    writer.WriteEndObject()
                | UnionField info ->
                    writer.WriteStartObject()
                    nameof UnionField |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, info, typeof<UnionFieldInfo>, options)
                    writer.WriteEndObject()
                | UnionTag -> nameof UnionTag |> prefix |> wrap |> (fun v -> writer.WriteRawValue(v, true))
                | ListHead -> nameof ListHead |> prefix |> wrap |> (fun v -> writer.WriteRawValue(v, true))
                | ListTail -> nameof ListTail |> prefix |> wrap |> (fun v -> writer.WriteRawValue(v, true))
                | OptionValue -> nameof OptionValue |> prefix |> wrap |> (fun v -> writer.WriteRawValue(v, true))
    type SetKindConverter() =
        inherit Serialization.JsonConverter<SetKind>()
        override this.CanConvert typ = typ = typeof<SetKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof SetKind}.{postfix}"
                let wrap input = $"\"{input}\""
                match value with
                | ExprSet expr ->
                    writer.WriteStartObject()
                    nameof ExprSet |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, expr, typeof<Expr>, options)
                    writer.WriteEndObject()
                | FieldSet fieldName ->
                    writer.WriteStartObject()
                    nameof FieldSet |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, fieldName, typeof<string>, options)
                    writer.WriteEndObject()
                | ValueSet -> nameof ValueSet |> prefix |> wrap |> (fun v -> writer.WriteRawValue(v, true))
    type TestKindConverter() =
        inherit Serialization.JsonConverter<TestKind>()
        override this.CanConvert typ = typ = typeof<TestKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                let prefix postfix = $"{nameof TestKind}.{postfix}"
                writer.WriteStartObject()
                match value with
                | TypeTest typ ->
                    nameof TypeTest |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, typ, typeof<Fable.Type>, options)
                | OptionTest isSome ->
                    nameof OptionTest |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, isSome, typeof<bool>, options)
                | ListTest isCons ->
                    nameof ListTest |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, isCons, typeof<bool>, options)
                | UnionCaseTest tag ->
                    nameof UnionCaseTest |> prefix |> writer.WritePropertyName
                    JsonSerializer.Serialize(writer, tag, typeof<int>, options)
                writer.WriteEndObject()
    type ExtendedSetConverter() =
        inherit Serialization.JsonConverter<ExtendedSet>()
        override this.CanConvert typ = typ = typeof<ExtendedSet>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_EXTENDEDSET\"", true)
    type UnresolvedExprConverter() =
        inherit Serialization.JsonConverter<UnresolvedExpr>()
        override this.CanConvert typ = typ = typeof<UnresolvedExpr>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_UNRESOLVEDEXPR\"", true)
    type OperationKindConverter() =
        inherit Serialization.JsonConverter<OperationKind>()
        override this.CanConvert typ = typ = typeof<OperationKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_OPERATIONKIND\"", true)
    type NumberKindConverter() =
        inherit Serialization.JsonConverter<NumberKind>()
        override this.CanConvert typ = typ = typeof<NumberKind>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_NumberKindConverter\"", true)
    type RegexFlagConverter() =
        inherit Serialization.JsonConverter<RegexFlag>()
        override this.CanConvert typ = typ = typeof<RegexFlag>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_RegexFlagConverter\"", true)
    type UnaryOperatorConverter() =
        inherit Serialization.JsonConverter<UnaryOperator>()
        override this.CanConvert typ = typ = typeof<UnaryOperator>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_UnaryOperatorConverter\"", true)
    type BinaryOperatorConverter() =
        inherit Serialization.JsonConverter<BinaryOperator>()
        override this.CanConvert typ = typ = typeof<BinaryOperator>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_BINARYOPERATOR\"")
    type LogicalOperatorConverter() =
        inherit Serialization.JsonConverter<LogicalOperator>()
        override this.CanConvert typ = typ = typeof<LogicalOperator>
        override this.Read(_, _, _) = unbox null
        override this.Write(writer, value, options) =
            if canWrite writer then
                writer.WriteRawValue("\"TODO_LOGICALOPERATOR\"")

    let options = JsonSerializerOptions(JsonSerializerDefaults.General)
    options.Converters.Add(EntityPathConverter())
    options.Converters.Add(MemberRefConverter())
    options.Converters.Add(GeneratedMemberConverter())
    options.Converters.Add(NumberInfoConverter())
    options.Converters.Add(NumberValueConverter())
    options.Converters.Add(ArrayKindConverter())
    options.Converters.Add(ConstraintConverter())
    options.Converters.Add(TypeConverter())
    options.Converters.Add(ExprConverter())
    options.Converters.Add(ValueKindConverter())
    options.Converters.Add(TagSourceConverter())
    options.Converters.Add(TagInfoConverter())
    options.Converters.Add(ImportKindConverter())
    options.Converters.Add(DeclarationConverter())
    options.Converters.Add(NewArrayKindConverter())
    options.Converters.Add(GetKindConverter())
    options.Converters.Add(SetKindConverter())
    options.Converters.Add(TestKindConverter())
    options.Converters.Add(ExtendedSetConverter())
    options.Converters.Add(UnresolvedExprConverter())
    options.Converters.Add(OperationKindConverter())
    options.Converters.Add(NumberKindConverter())
    options.Converters.Add(RegexFlagConverter())
    options.Converters.Add(UnaryOperatorConverter())
    options.Converters.Add(BinaryOperatorConverter())
    options.Converters.Add(LogicalOperatorConverter())

    // UN-UTILISED ATM
    /// ANSI Escape Sequences to colorize strings when directed to output.
    [<AutoOpen>]
    module private FColor =
        type private FColor = string
        /// When the output is not directed to console (ie directed to a file) then the escape sequence is not written
        let private _fcolor value =
            if Console.IsOutputRedirected then "" else value

        /// Applies Color to value only. Returns color to normal after.
        let colorize (color: FColor) (value: string) = $"{color}{value}{NORMAL}"
        // Presets
        let NORMAL: FColor = _fcolor "\x1b[39m" // Normal
        let REVERSE: FColor = _fcolor "\x1b[7m" // Inverts Foreground and Background colors
        let NOREVERSE: FColor = _fcolor "\x1b[27m" // Reverts Foreground and Background colors
        // Colors
        let RED: FColor = _fcolor "\x1b[91m"
        let GREEN: FColor = _fcolor "\x1b[92m"
        let YELLOW: FColor = _fcolor "\x1b[93m"
        let BLUE: FColor = _fcolor "\x1b[94m"
        let MAGENTA: FColor = _fcolor "\x1b[95m"
        let CYAN: FColor = _fcolor "\x1b[96m"
        let GREY: FColor = _fcolor "\x1b[97m"
        // Formatting
        let BOLD: FColor = _fcolor "\x1b[1m" // Intense
        let NOBOLD: FColor = _fcolor "\x1b[22m" // Normal intensity
        let UNDERLINE: FColor = _fcolor "\x1b[4m" // Underlined
        let NOUNDERLINE: FColor = _fcolor "\x1b[24m" // Remove any underline

// Public API point for plugin printing AST to string
type PrettyPrinter =
    static member print(value: 'T) : string =
        JsonSerializer.Serialize(value, PrettyPrint.options)

