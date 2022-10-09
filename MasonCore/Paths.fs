namespace MasonCore

module Paths =

    let mutable dataPath = "/data/"
    let staticContentPath() = $"{dataPath}staticContent/"
    let databasePath() = $"{dataPath}data.db"
    let configureDataPath path = dataPath <- path


    let mutable logPath = "/logs/"
    let logFilePath() = $"{logPath}log.txt"
    let configureLogPath path = logPath <- path


    let mutable secretsPath = "/secrets/"
    let telegramToken() = $"{secretsPath}telegramToken.txt"
    let configureSecretsPath path = secretsPath <- path
