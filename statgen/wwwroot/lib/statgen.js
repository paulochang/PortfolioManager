(async function() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/app")
        .build();

    const app = new Vue({
        el: "#app",
        data: {
            stockIndex: null,
            newStockPrice: null,
            header: "Shopping List App",
            stocks: [ ]
                /*
                {
                    symbol: "MSFT",
                    price: 75.54,
                    open: 75.12,
                    high: 75.54,
                    low: 75.12,
                    change: 0.42,
                    perc: 0.56

                },
                {
                    symbol: "AAPL",
                    price: 157.96,
                    open: 158.44,
                    high: 158.44,
                    low: 157.96,
                    change: -0.48,
                    perc: -0.30

                },
                {
                    symbol: "GOOG",
                    price: 923.42,
                    open: 924.54,
                    high: 925.37,
                    low: 923.42,
                    change: -1.12,
                    perc: -0.12
                }
                */
            },
        methods: {
            async addStockPriceChange() {
                        await fetch("/price", {
                            method: "POST",
                            body: JSON.stringify(
                                { 
                                    Index: this.stockIndex, 
                                    Price: this.newStockPrice
                                }),
                            headers: {
                                "content-type": "application/json"
                            }
                        })
                        this.newRestMessage = null;
                    }
        }
    });

    connection.on("Send", stock => {
        app.stocks.push(stock);
    });

    connection.on("Update", stock => {
        priceUpdate(stock);
    })

    var priceUpdate = function(stock) {
        let currentStock = app.stocks[stock.index];
        if (currentStock.price !== stock.price) {
            currentStock.lastChange = currentStock.price - stock.price;
            currentStock.price = stock.price;
            if (currentStock.open == 0) currentStock.open = stock.price;
            if (currentStock.low > stock.price || currentStock.low == 0 ) currentStock.low = stock.price;
            if (currentStock.high < stock.price) currentStock.high = stock.price;
            currentStock.change = currentStock.price-currentStock.open;
            currentStock.perc = (currentStock.change/currentStock.price).toFixed(2);
        }
    }

    await connection.start().then(function() {
        connection.invoke("GetAllStocks").then(function(stocks) {
            for (let i = 0; i < stocks.length; i++) {
                app.stocks.push(stocks[i]);
            }
        });
    });

})();