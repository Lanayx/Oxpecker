module Backend.Abstractions

open System
open System.Threading.Tasks
open FSharp.UMX
open Microsoft.Extensions.Logging

// abstractions
type IDbClient =
    abstract member ExecuteStatement: sql: string -> Task<'T>
    abstract member ExecuteTransaction: sqlStatements: string[] -> Task<unit>

type IDbEnv =
    abstract DbClient: IDbClient

type IAppLogger =
    abstract Logger: ILogger
