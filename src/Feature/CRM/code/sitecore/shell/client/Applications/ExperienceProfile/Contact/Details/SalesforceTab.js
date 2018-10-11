define(["sitecore", "/-/speak/v1/experienceprofile/DataProviderHelper.js", "/-/speak/v1/experienceprofile/CintelUtl.js"], function (sc, providerHelper, cintelUtil) {
    var intelPath = "/intel",
        dataSetProperty = "dataSet";

    var getTypeValue = function (preffered, all) {
        if (preffered.Key) {
            return { Key: preffered.Key, Value: preffered.Value };
        } else if (all.length > 0) {
            return { Key: all[0].Key, Value: all[0].Value };
        }

        return null;
    };

    var app = sc.Definitions.App.extend({
        initialized: function () {
            var transformers = $.map(
                [
                    "default"
                ], function (tableName) {
                    return { urlKey: intelPath + "/" + tableName + "?", headerValue: tableName };
                });

            providerHelper.setupHeaders(transformers);
            providerHelper.addDefaultTransformerKey();

            this.setupContactDetail();
        },

        setEmail: function (textControl, email) {
            if (email && email.indexOf("@") > -1) {
                cintelUtil.setText(textControl, "", true);
                textControl.viewModel.$el.html('<a href="mailto:' + email + '">' + email + '</a>');
            } else {
                cintelUtil.setText(textControl, email, true);
            }
        },

        setupContactDetail: function () {

            providerHelper.initProvider(this.ContactSalesforceDataProvider, "", sc.Contact.baseUrl, this.SalesforceTabMessageBar);
            providerHelper.getData(
                this.ContactSalesforceDataProvider,
                $.proxy(function (jsonData) {
                    this.ContactSalesforceDataProvider.set(dataSetProperty, jsonData);
                    var dataSet = this.ContactSalesforceDataProvider.get(dataSetProperty);
				
                    cintelUtil.setText(this.SalesforceContactCreatedDateValue, jsonData.SalesforceContactCreatedDate, false);
					cintelUtil.setText(this.CustomSalesforceIdValue, jsonData.identifier.find(x => x.Source === "Salesforce.ContactId").Identifier, false);	
                    cintelUtil.setText(this.CustomSalesforceJourneyStatusValue, jsonData.CustomSalesforceJourneyStatus, false);
                }, this)
            );


        }
    });
    return app;
});