namespace Tests

open Funogram.Telegram.Types
open Handlers.Basic
open NUnit.Framework
open Helpers
open Handlers

module StartHandlerTest =

    [<SetUp>]
    let Setup () =
        mockContent ()
        testMode <- TestMode.Enabled
        lastSentMessage <- None


    [<Test>]
    let ``Correct handling /start`` () =
        let msg = createTextMessage "/start"

        let handlingResult = StartHandler.handleStart msg

        let actual = lastSentMessage
        let expected = Some (Content.welcomeMessage, ParseMode.Markdown, Some StartHandler.keyboard)
        expectSuccess handlingResult
        Assert.AreEqual(actual, expected)

    [<Test>]
    let ``Correct handling not /start`` () =
        let msg = createTextMessage "thats no is /start"

        let handlingResult = StartHandler.handleStart msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(actual, expected)

    [<Test>]
    let ``Correct handling HowToMason callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StartHandler.handleHowToMason msg

        let actual = lastSentMessage
        let expected = Some (Content.howToMasonMessage, ParseMode.Markdown, Some StartHandler.keyboard)
        expectSuccess handlingResult
        Assert.AreEqual(actual, expected)

    [<Test>]
    let ``Correct handling not HowToMason callback`` () =
        let msg = createCallbackMessage Callback.WhatIsMason

        let handlingResult = StartHandler.handleHowToMason msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(actual, expected)

    [<Test>]
    let ``Correct handling WhatIsMason callback`` () =
        let msg = createCallbackMessage Callback.WhatIsMason

        let handlingResult = StartHandler.handleWhatIsMason msg

        let actual = lastSentMessage
        let expected = Some (Content.whatIsMasonMessage, ParseMode.Markdown, Some StartHandler.keyboard)
        expectSuccess handlingResult
        Assert.AreEqual(actual, expected)

    [<Test>]
    let ``Correct handling not WhatIsMason callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StartHandler.handleWhatIsMason msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(actual, expected)
