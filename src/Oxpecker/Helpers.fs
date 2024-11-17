namespace Oxpecker

open Microsoft.IO

[<AutoOpen>]
module Helpers =

    /// <summary>Default single RecyclableMemoryStreamManager.</summary>
    let internal recyclableMemoryStreamManager = Lazy<RecyclableMemoryStreamManager>()

    /// Same as << but with two arguments
    let inline (<<+) func2 func1 x y = func2(func1 x y)

    /// Same as << but with three arguments
    let inline (<<++) func2 func1 x y z = func2(func1 x y z)

    /// <summary>
    /// Utility function for matching 1xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 100 and 199.</returns>
    let is1xxStatusCode (statusCode: int) = 100 <= statusCode && statusCode <= 199

    /// <summary>
    /// Utility function for matching 2xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 200 and 299.</returns>
    let is2xxStatusCode (statusCode: int) = 200 <= statusCode && statusCode <= 299

    /// <summary>
    /// Utility function for matching 3xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 300 and 399.</returns>
    let is3xxStatusCode (statusCode: int) = 300 <= statusCode && statusCode <= 399

    /// <summary>
    /// Utility function for matching 4xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 400 and 499.</returns>
    let is4xxStatusCode (statusCode: int) = 400 <= statusCode && statusCode <= 499

    /// <summary>
    /// Utility function for matching 5xx HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Returns true if the status code is between 500 and 599.</returns>
    let is5xxStatusCode (statusCode: int) = 500 <= statusCode && statusCode <= 599

    /// <summary>
    /// Boxes value with return type obj (not objnull).
    /// </summary>
    let inline boxv<'a when 'a: struct> (v: 'a) = box v |> Unchecked.nonNull
