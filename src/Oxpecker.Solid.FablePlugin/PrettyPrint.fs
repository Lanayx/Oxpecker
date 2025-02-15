namespace Oxpecker.Solid.FablePlugin

open System
open Fable.AST
open Fable.AST.Fable
open Types
open System.Text.Json

module internal rec PrettyPrint =
    type EntityPathConverter() =
        inherit Serialization.JsonConverter<EntityPath>()
        override this.CanConvert typ = typ = typeof<EntityPath>
        override this.Read(_,_,_) = unbox null
        override this.Write( writer, value, options ) =
            let prefix postfix = $"{nameof EntityPath}.{postfix}"
            match value with
            | SourcePath s -> writer.WriteString (prefix (nameof SourcePath), s)
            | AssemblyPath s -> writer.WriteString (prefix (nameof AssemblyPath), s)
            | CoreAssemblyName s -> writer.WriteString (prefix (nameof CoreAssemblyName), s)
            | PrecompiledLib(sourcePath, assemblyPath) -> writer.WriteString (prefix (nameof PrecompiledLib), JsonSerializer.Serialize {|sourcePath = sourcePath; assemblyPath = assemblyPath|})
    type MemberRefConverter() =
        inherit Serialization.JsonConverter<MemberRef>()
        override this.CanConvert typ = typ = typeof<MemberRef>
        override this.Read(_,_,_) = unbox null
        override this.Write( writer, value, options) =
            let prefix postfix = $"{nameof MemberRef}.{postfix}"
            match value with
            | MemberRef(declaringEntity, info) ->
                let inp = {|declaringEntity=declaringEntity;info=info|}
                writer.WritePropertyName (prefix (nameof MemberRef))
                JsonSerializer.Serialize(writer, inp, inp.GetType(), options)
            | GeneratedMemberRef generatedMember ->
                nameof GeneratedMemberRef |> prefix |> writer.WritePropertyName
                JsonSerializer.Serialize(writer, generatedMember, generatedMember.GetType(), options)
    type GeneratedMemberConverter() =
        inherit Serialization.JsonConverter<GeneratedMember>()
        override this.CanConvert typ = typ = typeof<GeneratedMember>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let prefix postfix = $"{nameof GeneratedMember}.{postfix}"
            match value with
            | GeneratedFunction info ->
                let object = {|GeneratedFunction=info|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | GeneratedValue info ->
                let object = {|GeneratedValue=info|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | GeneratedGetter info ->
                let object = {|GeneratedGetter=info|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | GeneratedSetter info ->
                let object = {|GeneratedSetter=info|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
    type NumberInfoConverter() =
        inherit Serialization.JsonConverter<NumberInfo>()
        override this.CanConvert typ = typ = typeof<NumberInfo>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let prefix postfix = $"{nameof NumberInfo}.{postfix}"
            match value with
            | NumberInfo.Empty -> nameof NumberInfo.Empty |> prefix |> writer.WriteNull
            | NumberInfo.IsMeasure fullname -> nameof NumberInfo.IsMeasure |> prefix |> fun prop -> writer.WriteString (prop, fullname)
            | NumberInfo.IsEnum ent ->
                nameof NumberInfo.IsEnum |> prefix |> writer.WritePropertyName
                JsonSerializer.Serialize(writer, ent, ent.GetType(), options)
    type NumberValueConverter() =
        inherit Serialization.JsonConverter<NumberValue>()
        override this.CanConvert typ = typ = typeof<NumberValue>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            match value with
            | NumberValue.Int8 b -> JsonSerializer.Serialize(writer, b, b.GetType(), options)
            | NumberValue.UInt8 b -> JsonSerializer.Serialize(writer, b, b.GetType(), options)
            | NumberValue.Int16 s -> JsonSerializer.Serialize(writer, s, s.GetType(), options)
            | NumberValue.UInt16 s -> JsonSerializer.Serialize(writer, s, s.GetType(), options)
            | NumberValue.Int32 i -> JsonSerializer.Serialize(writer, i, i.GetType(), options)
            | NumberValue.UInt32 i -> JsonSerializer.Serialize(writer, i, i.GetType(), options)
            | NumberValue.Int64 int64 -> JsonSerializer.Serialize(writer, int64, int64.GetType(), options)
            | NumberValue.UInt64 uInt64 -> JsonSerializer.Serialize(writer, uInt64, uInt64.GetType(), options)
            | NumberValue.Int128(upper, lower) -> $"{upper}{lower}" |> fun v -> writer.WriteRawValue(v,true)
            | NumberValue.UInt128(upper, lower) -> $"{upper}{lower}" |> fun v -> writer.WriteRawValue(v,true)
            | NumberValue.BigInt bigInteger -> JsonSerializer.Serialize(writer, bigInteger, bigInteger.GetType(), options)
            | NumberValue.NativeInt intPtr -> JsonSerializer.Serialize(writer, intPtr, intPtr.GetType(), options)
            | NumberValue.UNativeInt uIntPtr -> JsonSerializer.Serialize(writer, uIntPtr, uIntPtr.GetType(), options)
            | NumberValue.Float16 f -> JsonSerializer.Serialize(writer, f, f.GetType(), options)
            | NumberValue.Float32 f -> JsonSerializer.Serialize(writer, f, f.GetType(), options)
            | NumberValue.Float64 f -> JsonSerializer.Serialize(writer, f, f.GetType(), options)
            | NumberValue.Decimal decimal -> JsonSerializer.Serialize(writer, decimal, decimal.GetType(), options)
    type ArrayKindConverter() =
        inherit Serialization.JsonConverter<ArrayKind>()
        override this.CanConvert typ = typ = typeof<ArrayKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            match value with
            | ResizeArray -> nameof ResizeArray
            | MutableArray -> nameof MutableArray
            | ImmutableArray -> nameof ImmutableArray
            |> fun value -> writer.WriteString (nameof ArrayKind,value)
    type ConstraintConverter() =
        inherit Serialization.JsonConverter<Constraint>()
        override this.CanConvert typ = typ = typeof<Constraint>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let wrap input = $"\"{input}\""
            writer.WriteStartObject()
            match value with
            | Constraint.HasMember(name, isStatic) ->
                writer.WritePropertyName $"{nameof Constraint}.{nameof Constraint.HasMember}"
                let object = {|name=name;isStatic=isStatic|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Constraint.CoercesTo target ->
                writer.WritePropertyName $"{nameof Constraint}.{nameof Constraint.CoercesTo}"
                // JsonSerializer.Serialize(writer, target, target.GetType(), options)
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
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let wrap input = $"\"{input}\""
            let prefix postfix = $"{nameof Fable.Type}.{postfix}"
            writer.WriteStartObject()
            match value with
            | Type.Measure fullname -> writer.WriteString(prefix (nameof Measure), fullname)
            | Type.MetaType -> let object = (nameof MetaType) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.Any -> let object = (nameof Any) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.Unit -> let object = (nameof Unit) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.Boolean -> let object = (nameof Boolean) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.Char -> let object = (nameof Char) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.String -> let object = (nameof String) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.Regex -> let object = (nameof Regex) |> prefix |> wrap in writer.WriteRawValue(object, true)
            | Type.Number(kind, info) ->
                writer.WritePropertyName (prefix (nameof Number))
                let object = {|kind=kind;info=info|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.Option(genericArg, isStruct) ->
                writer.WritePropertyName (prefix (nameof Option))
                let object = {|genericArg=genericArg;isStruct=isStruct|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.Tuple(genericArgs, isStruct) ->
                writer.WritePropertyName (prefix (nameof Tuple))
                let object = {|genericArgs=genericArgs;isStruct=isStruct|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.Array(genericArg, kind) ->
                writer.WritePropertyName (prefix (nameof Array))
                let object = {|genericArg=genericArg;kind=kind|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.List genericArg ->
                writer.WritePropertyName (prefix (nameof List))
                let object = {|genericArg=genericArg|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.LambdaType(argType, returnType) ->
                writer.WritePropertyName (prefix (nameof LambdaType))
                let object = {|argType=argType; returnType=returnType|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.DelegateType(argTypes, returnType) ->
                writer.WritePropertyName (prefix (nameof DelegateType))
                let object = {|argTypes=argTypes; returnType=returnType|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.GenericParam(name, isMeasure, constraints) ->
                writer.WritePropertyName (prefix (nameof GenericParam))
                let object = {|name=name;isMeasure=isMeasure;constraints=constraints|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.DeclaredType(rref, genericArgs) ->
                writer.WritePropertyName (prefix (nameof DeclaredType))
                let object = {|rref=rref;genericArgs=genericArgs|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Type.AnonymousRecordType(fieldNames, genericArgs, isStruct) ->
                writer.WritePropertyName (prefix (nameof AnonymousRecordType))
                let object = {|fieldNames=fieldNames;genericArgs=genericArgs;isStruct=isStruct|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            writer.WriteEndObject()
    type ExprConverter() =
        inherit Serialization.JsonConverter<Expr>()
        override this.CanConvert typ = typ = typeof<Expr>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let prefix postfix = $"{nameof Expr}.{postfix}"
            let wrap input = $"\"{input}\""
            writer.WriteStartObject()
            match value with
            | IdentExpr ident ->
                writer.WritePropertyName(prefix(nameof IdentExpr))
                JsonSerializer.Serialize(writer, ident, ident.GetType(), options)
            | Value(kind, range) ->
                writer.WritePropertyName(prefix(nameof Value))
                let object = {|kind=kind;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Lambda(arg, body, name) ->
                writer.WritePropertyName(prefix(nameof Lambda))
                let object = {|arg=arg;body=body;name=name|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Delegate(args, body, name, tags) ->
                writer.WritePropertyName(prefix(nameof Delegate))
                let object = {|args=args;body=body;name=name;tags=tags|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | ObjectExpr(members, typ, baseCall) ->
                writer.WritePropertyName(prefix(nameof ObjectExpr))
                let object = {|members=members;``type``=typ;baseCall=baseCall|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | TypeCast(expr, typ) ->
                writer.WritePropertyName(prefix(nameof TypeCast))
                let object = {|expr=expr;``type``=typ|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Test(expr, kind, range) ->
                writer.WritePropertyName(prefix(nameof Test))
                let object = {|expr=expr;kind=kind;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Call(callee, info, typ, range) ->
                writer.WritePropertyName(prefix(nameof Call))
                let object = {|callee=callee;info=info;``type``=typ;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | CurriedApply(applied, args, typ, range) ->
                writer.WritePropertyName(prefix(nameof CurriedApply))
                let object = {|applied=applied;args=args;``type``=typ;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Operation(kind, tags, typ, range) ->
                writer.WritePropertyName(prefix(nameof Operation))
                let object = {|kind=kind;tags=tags;``type``=typ;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Import(info, typ, range) ->
                writer.WritePropertyName(prefix(nameof Import))
                let object = {|info=info;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Emit(info, typ, range) ->
                writer.WritePropertyName(prefix(nameof Emit))
                let object = {|info=info;``type``=typ;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | DecisionTree(expr, targets) ->
                writer.WritePropertyName(prefix(nameof DecisionTree))
                let object = {|expr=expr;targets=targets|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | DecisionTreeSuccess(targetIndex, boundValues, typ) ->
                writer.WritePropertyName(prefix(nameof DecisionTreeSuccess))
                let object = {|targetIndex=targetIndex;boundValues=boundValues;``type``=typ|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Let(ident, value, body) ->
                writer.WritePropertyName(prefix(nameof Let))
                let object = {|ident=ident;value=value;body=body|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | LetRec(bindings, body) ->
                writer.WritePropertyName(prefix(nameof LetRec))
                let object = {|bindings=bindings;body=body|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Get(expr, kind, typ, range) ->
                writer.WritePropertyName(prefix(nameof Get))
                let object = {|expr=expr;kind=kind;``type``=typ;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Set(expr, kind, typ, value, range) ->
                writer.WritePropertyName(prefix(nameof Set))
                let object = {|expr=expr;kind=kind;``type``=typ;value=value;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Sequential exprs ->
                writer.WritePropertyName(prefix(nameof Sequential))
                let object = exprs
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | WhileLoop(guard, body, range) ->
                writer.WritePropertyName(prefix(nameof WhileLoop))
                let object = {|guard=guard;body=body;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | ForLoop(ident, start, limit, body, isUp, range) ->
                writer.WritePropertyName(prefix(nameof ForLoop))
                let object = {|ident=ident;start=start;limit=limit;body=body;isUp=isUp;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | TryCatch(body, catch, finalizer, range) ->
                writer.WritePropertyName(prefix(nameof TryCatch))
                let object = {|body=body;catch=catch;finalizer=finalizer;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | IfThenElse(guardExpr, thenExpr, elseExpr, range) ->
                writer.WritePropertyName(prefix(nameof IfThenElse))
                let object = {|guardExpr=guardExpr;thenExpr=thenExpr;elseExpr=elseExpr;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Unresolved(expr, typ, range) ->
                writer.WritePropertyName(prefix(nameof Unresolved))
                let object = {|expr=expr;``type``=typ;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Extended(expr, range) ->
                writer.WritePropertyName(prefix(nameof Extended))
                let object = {|expr=expr;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            writer.WriteEndObject()
    type ValueKindConverter() =
        inherit Serialization.JsonConverter<ValueKind>()
        override this.CanConvert typ = typ = typeof<ValueKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
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
                let object = {|boundIdent=boundIdent;``type``=typ|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | TypeInfo(typ, tags) ->
                nameof TypeInfo |> prefix |> wrap |> writer.WritePropertyName
                let object = {|``type``=typ;tags=tags|}
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
                let object = {|tag=tag;parts=parts;values=values|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NumberConstant(value, info) ->
                nameof NumberConstant |> prefix |> wrap |> writer.WritePropertyName
                let object = {|value=value;info=info|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | RegexConstant(source, flags) ->
                nameof RegexConstant |> prefix |> wrap |> writer.WritePropertyName
                let object = {|source=source;flags=flags|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewOption(value, typ, isStruct) ->
                nameof NewOption |> prefix |> wrap |> writer.WritePropertyName
                let object = {|value=value;``type``=typ;isStruct=isStruct|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewArray(newKind, typ, kind) ->
                nameof NewArray |> prefix |> wrap |> writer.WritePropertyName
                let object = {|newKind=newKind;``type``=typ;kind=kind|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewList(headAndTail, typ) ->
                nameof NewList |> prefix |> wrap |> writer.WritePropertyName
                let object = {|headAndTail=headAndTail;``type``=typ|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewTuple(values, isStruct) ->
                nameof NewTuple |> prefix |> wrap |> writer.WritePropertyName
                let object = {|values=values;isStruct=isStruct|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewRecord(values, rref, genArgs) ->
                nameof NewRecord |> prefix |> wrap |> writer.WritePropertyName
                let object = {|values=values;rref=rref;genArgs=genArgs|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewAnonymousRecord(values, fieldNames, genArgs, isStruct) ->
                nameof NewAnonymousRecord |> prefix |> wrap |> writer.WritePropertyName
                let object = {|values=values;fieldNames=fieldNames;genArgs=genArgs;isStruct=isStruct|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NewUnion(values, tag, rref, genArgs) ->
                nameof NewUnion |> prefix |> wrap |> writer.WritePropertyName
                let object = {|values=values;tag=tag;rref=rref;genArgs=genArgs|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            writer.WriteEndObject()
    type TagSourceConverter() =
        inherit Serialization.JsonConverter<TagSource>()
        override this.CanConvert typ = typ = typeof<TagSource>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let prefix postfix = $"{nameof TagSource}.{postfix}"
            writer.WriteStartObject()
            match value with
            | AutoImport tagName -> writer.WriteString(prefix (nameof AutoImport), tagName)
            | LibraryImport imp ->
                writer.WritePropertyName (prefix (nameof LibraryImport))
                JsonSerializer.Serialize(writer, imp, typeof<Expr>, options)
            writer.WriteEndObject()
    type TagInfoConverter() =
        inherit Serialization.JsonConverter<TagInfo>()
        override this.CanConvert typ = typ = typeof<TagInfo>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            let prefix postfix = $"{nameof TagInfo}.{postfix}"
            writer.WriteStartObject()
            match value with
            | WithChildren(tagName, propsAndChildren, range) ->
                nameof WithChildren |> prefix |> writer.WritePropertyName
                let object = {|tagName=tagName;propsAndChildren=propsAndChildren;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | NoChildren(tagName, props, range) ->
                nameof NoChildren |> prefix |> writer.WritePropertyName
                let object = {|tagName=tagName;props=props;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            | Combined(tagName, props, propsAndChildren, range) ->
                nameof Combined |> prefix |> writer.WritePropertyName
                let object = {|tagName=tagName;props=props;propsAndChildren=propsAndChildren;range=range|}
                JsonSerializer.Serialize(writer, object, object.GetType(), options)
            writer.WriteEndObject()
    type ImportKindConverter() =
        inherit Serialization.JsonConverter<ImportKind>()
        override this.CanConvert typ = typ = typeof<ImportKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type DeclarationConverter() =
        inherit Serialization.JsonConverter<Declaration>()
        override this.CanConvert typ = typ = typeof<Declaration>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type NewArrayKindConverter() =
        inherit Serialization.JsonConverter<NewArrayKind>()
        override this.CanConvert typ = typ = typeof<NewArrayKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type GetKindConverter() =
        inherit Serialization.JsonConverter<GetKind>()
        override this.CanConvert typ = typ = typeof<GetKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type SetKindConverter() =
        inherit Serialization.JsonConverter<SetKind>()
        override this.CanConvert typ = typ = typeof<SetKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type TestKindConverter() =
        inherit Serialization.JsonConverter<TestKind>()
        override this.CanConvert typ = typ = typeof<TestKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type ExtendedSetConverter() =
        inherit Serialization.JsonConverter<ExtendedSet>()
        override this.CanConvert typ = typ = typeof<ExtendedSet>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type UnresolvedExprConverter() =
        inherit Serialization.JsonConverter<UnresolvedExpr>()
        override this.CanConvert typ = typ = typeof<UnresolvedExpr>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type OperationKindConverter() =
        inherit Serialization.JsonConverter<OperationKind>()
        override this.CanConvert typ = typ = typeof<OperationKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type NumberKindConverter() =
        inherit Serialization.JsonConverter<NumberKind>()
        override this.CanConvert typ = typ = typeof<NumberKind>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type RegexFlagConverter() =
        inherit Serialization.JsonConverter<RegexFlag>()
        override this.CanConvert typ = typ = typeof<RegexFlag>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type UnaryOperatorConverter() =
        inherit Serialization.JsonConverter<UnaryOperator>()
        override this.CanConvert typ = typ = typeof<UnaryOperator>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type BinaryOperatorConverter() =
        inherit Serialization.JsonConverter<BinaryOperator>()
        override this.CanConvert typ = typ = typeof<BinaryOperator>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()
    type LogicalOperatorConverter() =
        inherit Serialization.JsonConverter<LogicalOperator>()
        override this.CanConvert typ = typ = typeof<LogicalOperator>
        override this.Read(_,_,_) = unbox null
        override this.Write(writer, value, options) =
            ()

    let options = JsonSerializerOptions(JsonSerializerDefaults.General)
    options.Converters.Add (EntityPathConverter())
    options.Converters.Add (MemberRefConverter())
    options.Converters.Add (GeneratedMemberConverter())
    options.Converters.Add (NumberInfoConverter())
    options.Converters.Add (NumberValueConverter())
    options.Converters.Add (ArrayKindConverter())
    options.Converters.Add (ConstraintConverter())
    options.Converters.Add (TypeConverter())
    options.Converters.Add (ExprConverter())
    options.Converters.Add (ValueKindConverter())
    options.Converters.Add (TagSourceConverter())
    options.Converters.Add (TagInfoConverter())
    options.Converters.Add (ImportKindConverter())
    options.Converters.Add (DeclarationConverter())
    options.Converters.Add (NewArrayKindConverter())
    options.Converters.Add (GetKindConverter())
    options.Converters.Add (SetKindConverter())
    options.Converters.Add (TestKindConverter())
    options.Converters.Add (ExtendedSetConverter())
    options.Converters.Add (UnresolvedExprConverter())
    options.Converters.Add (OperationKindConverter())
    options.Converters.Add (NumberKindConverter())
    options.Converters.Add (RegexFlagConverter())
    options.Converters.Add (UnaryOperatorConverter())
    options.Converters.Add (BinaryOperatorConverter())
    options.Converters.Add (LogicalOperatorConverter())
    /// ANSI Escape Sequences to colorize strings when directed to output.
    [<AutoOpen>]
    module private FColor =
        type private FColor = string
        /// When the output is not directed to console (ie directed to a file) then the escape sequence is not written
        let private _fcolor value = if Console.IsOutputRedirected then "" else value

        /// Applies Color to value only. Returns color to normal after.
        let colorize (color: FColor) (value: string) = $"{color}{value}{NORMAL}"
        // Presets
        let NORMAL : FColor =_fcolor "\x1b[39m" // Normal
        let REVERSE : FColor =_fcolor "\x1b[7m" // Inverts Foreground and Background colors
        let NOREVERSE : FColor =_fcolor "\x1b[27m" // Reverts Foreground and Background colors
        // Colors
        let RED : FColor =_fcolor "\x1b[91m"
        let GREEN : FColor =_fcolor "\x1b[92m"
        let YELLOW : FColor =_fcolor "\x1b[93m"
        let BLUE : FColor =_fcolor "\x1b[94m"
        let MAGENTA : FColor =_fcolor "\x1b[95m"
        let CYAN : FColor =_fcolor "\x1b[96m"
        let GREY : FColor =_fcolor "\x1b[97m"
        // Formatting
        let BOLD : FColor =_fcolor "\x1b[1m" // Intense
        let NOBOLD : FColor =_fcolor "\x1b[22m" // Normal intensity
        let UNDERLINE : FColor =_fcolor "\x1b[4m" // Underlined
        let NOUNDERLINE : FColor =_fcolor "\x1b[24m" // Remove any underline
    type PrettyPrinter =
        static member print(value: 'T): string = JsonSerializer.Serialize(value, options)
