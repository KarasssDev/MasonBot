namespace Tests

open Funogram.Telegram.Types
open Handlers.Basic
open NUnit.Framework

open Helpers
open Handlers

module StaticContentHandlersTests =

    [<SetUp>]
    let Setup () =
        mockContent ()
        testMode <- TestMode.Enabled
        lastSentMessage <- None


    [<Test>]
    let ``Correct handling /start`` () =
        let msg = createTextMessage "/start"

        let handlingResult = StaticContentHandlers.handleStart msg

        let actual = lastSentMessage
        let expected = Some (Content.welcomeMessage, ParseMode.Markdown, Some StaticContentHandlers.welcomeKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not /start`` () =
        let msg = createTextMessage "thats no is /start"

        let handlingResult = StaticContentHandlers.handleStart msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling HowToMason callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StaticContentHandlers.handleHowToMason msg

        let actual = lastSentMessage
        let expected = Some (Content.howToMasonMessage, ParseMode.Markdown, Some StaticContentHandlers.howToMasonKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not HowToMason callback`` () =
        let msg = createCallbackMessage Callback.WhatIsMason

        let handlingResult = StaticContentHandlers.handleHowToMason msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling WhatIsMason callback`` () =
        let msg = createCallbackMessage Callback.WhatIsMason

        let handlingResult = StaticContentHandlers.handleWhatIsMason msg

        let actual = lastSentMessage
        let expected = Some (Content.whatIsMasonMessage, ParseMode.Markdown, Some StaticContentHandlers.welcomeKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not WhatIsMason callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StaticContentHandlers.handleWhatIsMason msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling Start callback`` () =
        let msg = createCallbackMessage Callback.Start

        let handlingResult = StaticContentHandlers.handleStart msg

        let actual = lastSentMessage
        let expected = Some (Content.welcomeMessage, ParseMode.Markdown, Some StaticContentHandlers.welcomeKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not Start callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StaticContentHandlers.handleStart msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling BuyTon callback`` () =
        let msg = createCallbackMessage Callback.BuyTon

        let handlingResult = StaticContentHandlers.handleBuyTon msg

        let actual = lastSentMessage
        let expected = Some (Content.buyTonMessage, ParseMode.Markdown, Some StaticContentHandlers.howToMasonKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not BuyTon callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StaticContentHandlers.handleBuyTon msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling AboutNFT callback`` () =
        let msg = createCallbackMessage Callback.AboutNFT

        let handlingResult = StaticContentHandlers.handleAboutNFT msg

        let actual = lastSentMessage
        let expected = Some (Content.aboutNftMessage, ParseMode.Markdown, Some StaticContentHandlers.howToMasonKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not AboutNFT callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StaticContentHandlers.handleAboutNFT msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling BuyMasonNft callback`` () =
        let msg = createCallbackMessage Callback.BuyMasonNFT

        let handlingResult = StaticContentHandlers.handleBuyMasonNFT msg

        let actual = lastSentMessage
        let expected = Some (Content.buyMasonNftMessage, ParseMode.Markdown, Some StaticContentHandlers.howToMasonKeyboard)
        expectSuccess handlingResult
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``Correct handling not BuyMasonNft callback`` () =
        let msg = createCallbackMessage Callback.HowToMason

        let handlingResult = StaticContentHandlers.handleBuyMasonNFT msg

        let actual = lastSentMessage
        let expected = None
        expectFail handlingResult
        Assert.AreEqual(expected, actual)
