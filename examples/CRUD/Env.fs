module CRUD.Env

open CRUD.Models
open Microsoft.Extensions.Logging

type Env = {
    Logger: ILogger
    DbClient: IDbClient
} with
    interface IDbEnv with
        member this.DbClient = this.DbClient
    interface IAppLogger with
        member this.Logger = this.Logger
