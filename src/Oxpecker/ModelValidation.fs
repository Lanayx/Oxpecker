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
                (let dict = Dictionary<string, ResizeArray<string | null>>()
                 for error in errors do
                     for memberName in error.MemberNames do
                         match dict.TryGetValue(memberName) with
                         | true, value -> value.Add(error.ErrorMessage)
                         | false, _ ->
                             let arrayList = ResizeArray(1)
                             arrayList.Add(error.ErrorMessage)
                             dict[memberName] <- arrayList
                 dict)
        /// <summary>
        /// Get all validation results for a model.
        /// </summary>
        member this.All: ValidationResult seq = errors
        /// <summary>
        /// Get all error messages for a specific model field (could to be used with `nameof` funciton).
        /// </summary>
        member this.ErrorMessagesFor(name) : seq<string | null> =
            match errorDict.Value.TryGetValue(name) with
            | true, value -> value
            | false, _ -> Array.empty

    type InvalidModel<'T> = 'T * ValidationErrors

    /// <summary>
    /// Model wrapper for convenient usage with Oxpecker.ViewEngine
    /// </summary>
    [<RequireQualifiedAccess>]
    type ModelState<'T> =
        /// Could be used for "create" views.
        | Empty
        /// Could be used for "edit" views.
        | Valid of 'T
        /// Could be used for displaying errors in "create" and "edit" views.
        | Invalid of InvalidModel<'T>
        /// <summary>
        /// Pass an accessor function to get the string value of a model field (could be used with shorthand lambda).
        /// </summary>
        member this.Value(f: 'T -> string | null) =
            match this with
            | Empty -> null
            | Valid model -> f model
            | Invalid(model, _) -> f model

        /// <summary>
        /// Pass an accessor function to get the boolean value of a model field (could be used with shorthand lambda).
        /// </summary>
        member this.BoolValue(f: 'T -> bool) =
            match this with
            | Empty -> false
            | Valid model -> f model
            | Invalid(model, _) -> f model

    [<RequireQualifiedAccess>]
    type ModelValidationResult<'T> =
        | Valid of 'T
        | Invalid of InvalidModel<'T>

    /// <summary>
    /// Manually validate an object of type 'T`.
    /// </summary>
    let validateModel (model: 'T) =
        let validationResults = ResizeArray()
        match Validator.TryValidateObject(model, ValidationContext(model), validationResults, true) with
        | true -> ModelValidationResult.Valid model
        | false -> ModelValidationResult.Invalid(model, ValidationErrors(validationResults))

    /// <summary>
    /// Manually validate an object of type 'T` with a provided <see cref="System.ComponentModel.DataAnnotations.ValidationContext"/>.
    /// <param name="ctx">The validation context object.</param>
    /// <param name="model">The model object to validate.</param>
    /// </summary>
    let validateModelWith (ctx: ValidationContext) (model: 'T) =
        let validationResults = ResizeArray()
        match Validator.TryValidateObject(model, ctx, validationResults, true) with
        | true -> ModelValidationResult.Valid model
        | false -> ModelValidationResult.Invalid(model, ValidationErrors(validationResults))

    type HttpContextExtensions =

        /// <summary>
        /// Uses the <see cref="Serializers.IJsonSerializer"/> to deserialize the entire body of the <see cref="Microsoft.AspNetCore.Http.HttpRequest"/> asynchronously into an object of type 'T and then validate it.
        /// </summary>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
        [<Extension>]
        static member BindAndValidateJson<'T when 'T: not null>(ctx: HttpContext) =
            task {
                let! result = ctx.BindJson<'T>()
                let validationContext = ValidationContext(result, ctx.RequestServices, ctx.Items)
                return validateModelWith validationContext result
            }

        /// <summary>
        /// Parses all input elements from an HTML form into an object of type 'T and then validates it.
        /// </summary>
        /// <param name="ctx">The current http context object.</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns a <see cref="System.Threading.Tasks.Task{T}"/></returns>
        [<Extension>]
        static member BindAndValidateForm<'T when 'T: not null>(ctx: HttpContext) =
            task {
                let! result = ctx.BindForm<'T>()
                let validationContext = ValidationContext(result, ctx.RequestServices, ctx.Items)
                return validateModelWith validationContext result
            }

        /// <summary>
        /// Parses all parameters of a request's query string into an object of type 'T and then validates it.
        /// </summary>
        /// <param name="ctx">The current http context object.</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>Returns an instance of type 'T</returns>
        [<Extension>]
        static member BindAndValidateQuery<'T when 'T: not null>(ctx: HttpContext) =
            let result = ctx.BindQuery<'T>()
            let validationContext = ValidationContext(result, ctx.RequestServices, ctx.Items)
            validateModelWith validationContext result

    [<AutoOpen>]
    module RequestHandlers =
        /// <summary>
        /// Parses a JSON payload into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type ValidationResult<'T> and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateJson<'T when 'T: not null>
            (f: ModelValidationResult<'T> -> EndpointHandler)
            : EndpointHandler =
            fun (ctx: HttpContext) ->
                task {
                    let! model = ctx.BindJson<'T>()
                    let validationContext = ValidationContext(model, ctx.RequestServices, ctx.Items)
                    return! f (validateModelWith validationContext model) ctx
                }

        /// <summary>
        /// Parses a HTTP form payload into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateForm<'T when 'T: not null>
            (f: ModelValidationResult<'T> -> EndpointHandler)
            : EndpointHandler =
            fun (ctx: HttpContext) ->
                task {
                    let! model = ctx.BindForm<'T>()
                    let validationContext = ValidationContext(model, ctx.RequestServices, ctx.Items)
                    return! f (validateModelWith validationContext model) ctx
                }

        /// <summary>
        /// Parses a HTTP query string into an instance of type 'T and validates it.
        /// </summary>
        /// <param name="f">A function which accepts an object of type 'T and returns a <see cref="EndpointHandler"/> function.</param>
        /// <param name="ctx">HttpContext</param>
        /// <typeparam name="'T"></typeparam>
        /// <returns>An Oxpecker <see cref="EndpointHandler"/> function which can be composed into a bigger web application.</returns>
        let bindAndValidateQuery<'T when 'T: not null>
            (f: ModelValidationResult<'T> -> EndpointHandler)
            : EndpointHandler =
            fun (ctx: HttpContext) ->
                let model = ctx.BindQuery<'T>()
                let validationContext = ValidationContext(model, ctx.RequestServices, ctx.Items)
                f (validateModelWith validationContext model) ctx
