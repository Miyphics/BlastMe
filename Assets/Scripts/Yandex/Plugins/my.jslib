mergeInto(LibraryManager.library, {

    GetPlayerData: function () {
        if (player == null)
            return;

        myGameInstance.SendMessage("Yandex", "SetPlayerName", player.getName());
        myGameInstance.SendMessage("Yandex", "SetPlayerPhoto", player.getPhoto("small"));
    },

    RateGame: function () {
        if (player == null)
            return;

        ysdk.feedback.canReview()
            .then(({ value, reason }) => {
                if (value) {
                    ysdk.feedback.requestReview()
                        .then(({ feedbackSent }) => {
                            console.log(feedbackSent);
                            myGameInstance.SendMessage("Yandex", "SetPlayerName", player.getName());
                        })
                } else {
                    console.log(reason)
                }
            })
    },

    SaveGameExtern: function (data, flush) {
        if (player == null)
            return;

        var dataString = UTF8ToString(data);
        var myObj = JSON.parse(dataString);
        player.setData(myObj, flush);
    },

    LoadGameExtern: function () {
        if (player == null)
            return;

        player.getData().then(data => {
            const myJson = JSON.stringify(data);
            myGameInstance.SendMessage("PlayerManager", "LoadSaveJson", myJson);
        });
    },

    SetToLeaderboard: function (countOfLines, score) {
        ysdk.getLeaderboards()
            .then(lb => {
                switch (countOfLines) {
                    case 1:
                        lb.setLeaderboardScore("OneLine", score);
                        break;

                    case 2:
                        lb.setLeaderboardScore("TwoLines", score);
                        break;

                    case 3:
                        lb.setLeaderboardScore("ThreeLines", score);
                        break;
                }
            });
    },

    ShowAdsExtern: function (makeAuth) {
        ysdk.adv.showFullscreenAdv({
            callbacks: {
                onClose: function (wasShown) {
                    // some action after close
                    console.log("Adv Closed");

                    if (makeAuth == 1) {
                        myGameInstance.SendMessage("GameManager", "CheckAuth");
                    }
                },
                onError: function (error) {
                    // some action on error
                }
            }
        })
    },

    ShowAdvExtern: function () {
        ysdk.adv.showRewardedVideo({
            callbacks: {
                onOpen: () => {
                    console.log('Video ad open.');
                },
                onRewarded: () => {
                    console.log('Rewarded!');
                    myGameInstance.SendMessage('YandexAds', 'AddCoins', 1);
                },
                onClose: () => {
                    console.log('Video ad closed.');
                },
                onError: (e) => {
                    console.log('Error while open video ad:', e);
                    myGameInstance.SendMessage('YandexAds', 'AddCoins', 0);
                }
            }
        })
    },

    GetAuthExtern: function () {
        initPlayer().then(_player => {
            if (_player.getMode() === 'lite') {
                // Игрок не авторизован.
                myGameInstance.SendMessage("GameManager", "SetAuth", 0);
                return;
            }

            myGameInstance.SendMessage("GameManager", "SetAuth", 1);

            if (lb == null) {
                initLeaderboards();
            }

        }).catch(err => {
            // Ошибка при инициализации объекта Player.
            myGameInstance.SendMessage("GameManager", "SetAuth", 0);
        });
    },

    AuthExtern: function () {
        initPlayer().then(_player => {
            if (_player.getMode() === 'lite') {
                // Игрок не авторизован.
                ysdk.auth.openAuthDialog().then(() => {
                    // Игрок успешно авторизован

                    myGameInstance.SendMessage("GameManager", "SetAuth", 1);

                    if (lb == null)
                        initLeaderboards();

                    initPlayer().catch(err => {
                        // Ошибка при инициализации объекта Player.
                        myGameInstance.SendMessage("GameManager", "SetAuth", 0);
                    });
                }).catch(() => {
                    // Игрок не авторизован.
                    myGameInstance.SendMessage("GameManager", "SetAuth", 0);
                });
            }
        }).catch(err => {
            // Ошибка при инициализации объекта Player.
            myGameInstance.SendMessage("GameManager", "SetAuth", 0);
        });
    },

    GetLang: function () {
        var returnStr = ysdk.environment.i18n.lang;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },
});