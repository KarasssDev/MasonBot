namespace Logging

module Logging =
    open System

    let Error = 1
    let Warning = 2
    let Info = 3
    let Debug = 4

    let mutable current_log_level = Info
    let mutable current_text_writer = Console.Out
    let public configureWriter writer = current_text_writer <- writer
    let public configureLogLevel lvl = current_log_level <- lvl

    let LevelToString = function
        | 1 -> "Error"
        | 2 -> "Warning"
        | 3 -> "Info"
        | 4 -> "Debug"
        | _ -> "Unknown"

    let private writeLineString vLevel message =
        let res = sprintf "[%s] [%A] %s" (LevelToString vLevel) DateTime.Now message
        current_text_writer.WriteLine(res)
        current_text_writer.Flush()

    let public printLog vLevel format =
        Printf.ksprintf (fun message -> if current_log_level >= vLevel then writeLineString vLevel message) format

    let public printLogLazy vLevel format (s : Lazy<_>) =
        if current_log_level >= vLevel then
            Printf.ksprintf (writeLineString vLevel) format (s.Force())

    let public logError format = printLog Error format
    let public logWarning format = printLog Warning format
    let public logInfo format = printLog Info format
    let public logDebug format = printLog Debug format
