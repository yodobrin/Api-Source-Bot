

## Overview
The bot search service would provide customers looking for information about specific molecules (or API) informative details which will assist them in moving forward with a purchase decision. The interactive mechanism would interactive communication in real time. The bot would direct the conversation and attempt to obtain details about the customer to create leads in Salesforce.
The bot information is deemed as non CDA.
The name of the Bot - Sophi

## Architecture & Design

### Narrative Description
Teva-Tapi would implement together with MS a pilot bot to address search capabilities currently provided to minimal extent by CMS. The business has deemed the search of information about API to be cumbersome, and frustrating to some extent, and wishes to alter the approach. Discussions with account managers had revealed what are customers are most interested in. The initial version of the bot would address these findings. Having said that the solution would enable the business to understand what and how customers are searching for. It will provide mechanism to iron out what products are mostly searched and if specific ways are used. Using Language understanding by Microsoft would also allow for fast turnaround allowing new phrases and keywords to be recognized with minimal to none code changes. Technology used in the pilot would be enterprise grade making sure that future use cases can be built on the same platform.

#### Sample script – Customer Search an API
A customer navigates to the bot page: 
Bot: Hi, how can I help?
Customer: find me information about <API>
Bot: Here is the information I found about <API> {list of features}
Bot: Would you like me to send it to you by mail?
Customer: Yes, please
Bot: Great, can you provide your details: name/company/email?


### Architecture Diagram
![High Level Architecture](https://user-images.githubusercontent.com/37622785/50393654-4b1da680-0760-11e9-8f9b-9a4a26afce3a.png)


### Components
This section outlines per component its functionality, and reasoning for use in the overall solution.

#### Bot Services & App Service
A bot is an application interacting with users in a conversational manner. It is built on the foundation of app-service. More information on Bot Services can be found [here](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-3.0). App service further information can be found [here](https://docs.microsoft.com/en-us/azure/app-service/)
In our solution the bot would be sending customer typed messages to LUIS and after obtaining the intent and entities handle each sentence. It will interact with Azure search based on the scenario identified. The bot would also be able to send messages to the service bus, in case the intent recognized deem it as required. Bot will be able to leverage Bing Spell feature, in the appropriate scenario.

#### Azure Search
Azure search is providing index capabilities allowing fast search on multiple features of each document. More information can be found [here](https://docs.microsoft.com/en-us/azure/search/search-what-is-azure-search).
In our solution both would be providing the underline search capabilities together with maintaining the required information per API.
There are two types of search conducted, one will use the ‘any’ to find multiple results, or ‘all’ when specific query is required.

#### Cognitive Services - LUIS
Cloud based service leverage custom machine learning to understand conversations. Used to complete tasks. More information can be found [here](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/home).
In our context, we will train LUIS to understand several customer intents such as 
•	Search a product (API)
•	Provide more insights on products
LUIS will be trained to recognize the different API names, and their written variations. LUIS will extract the intent and entities from each user typed message and send to be processed by the bot service. We leverage LUIS in our solution to provide fast language understanding intent and entities extraction. It may be leveraged further for example to address sentiment from a conversation. 
Product list is broken to 2 main list of phrases, one consist of the entire name of the product and the other of the broken into words.

#### Bing Spell Check v7
Spell check is used in specific scenarios where the typed text is not recognized by LUIS to have a specific intent. If the altered text is identical to the original text, a message would be presented to the user.

#### Service Bus
A fully managed message broker. Used to decouple applications and allow asynchronous calls. More information can be found [here](https://docs.microsoft.com/en-us/azure/service-bus-messaging/).
In the context of our solution, it would be used to allow messages from the bot to be consumed by the logic app. The number of topics would be determined based on the number of different activities required. 
There are few queues created one per message type. In case a scenario calls for two type of messages, two distinct messages are created.


#### Logic App
Integration platform allowing automation of tasks together with prebuilt numerous interfaces and triggers. More info can be found [here](https://docs.microsoft.com/en-us/azure/logic-apps/).
In the context of this solution we will have few instances of logic apps. Logic app was selected for its ease of use, ability to integrate to multiple applications and systems (on-prem and on cloud) together with multiple invocation triggers.

##### Scheduled Information Extract - Importer
The information required to be indexed and searched by the bot would be extracted via a logic app instance, accessing Tapi back office tools such as Salesforce and BI. The logic app, receive JSON file from Tapi business personal, and builds a JSON entry per API. It will then after insert/update the entry for the API as blobs. This is a manual process, that leverage Logic App as the execution.

##### Lead & Survey Creation – Persist App & Lead Persist App
Upon specific cases the bot would create a message and publish it to the service bus; the logic apps would pull for new messages based on configurable amount of time. Once triggered the message would be processed either in one of the two ways
•	Save the lead to a csv file (append it to an existing file)
•	Save the survey result to a csv file (append to an existing file)
There is no validation on the content of the request as long as it with the format anticipated.

##### Communication Broker – Dist App & Transient App
Due to several technical limitations within Teva/Tapi, it was decided to send communication via GMAIL. The Dist App, sends the survey and lead files to a designated email address, upon request. The request is not secured. (in the future it needs to be)
The transient app, will send emails to customers based on the email provided within the bot.
There are two types of emails, one is for the catalog, where the app loads the file creates an email, add the catalog as an attachment and sends. The second type of email is for sending the searched information.






