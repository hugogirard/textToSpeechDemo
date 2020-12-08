# What this demo about

This demo expose an Azure Architecture using Azure Cognitive Services to convert text to audio.  The user enter a text in a Blazor Server web app.  A job is created and an azure function is triggered, the function will call Azure Cognitive Services to detect the text language (currently french and english are supported).

When the text is detected another function will be triggered and will call again Azure Cognitive Services to conver the text to audio file and save it in a blob.

# Architecture Diagram

<image src='https://github.com/hugogirard/textToSpeechDemo/blob/main/images/textToSpeech.png?raw=true'/>

### Flow

<ol>
    <li>The user navigate to the web app.</li>
    <li>The user is redirected to Azure AD for authentication.</li>
    <li>The user navigate to the job page, from there the user create a new job entering the text that it wants to convert to audio file.</li>
    <li>The job api create a new job in CosmosDB associated to the authenticated user.</li>
    <li>Once the job is created in CosmosDB a new item is inserted in the queue to analyze the text.</li>
    <li>An Azure Function kick in, the function call the Cognitive Service to analyze the text of the language.  Only french or english is supported at this moment</li>
    <li>    
        Once the cognitive service return the result a new item is inserted in a queue and the job status is updated in CosmosDB.    
    </li>
    <li>
        Another Azure Function start, this one create the audio spec based on the language of the text.
    </li>
    <li>
        The Azure Function call the Cognitive Service to convert the text to an audio file.
    </li>
    <li>
        The Azure Function update the job status in CosmosDB
    </li>
    <li>
        The Azure Function save the audio file in Azure Storage.
    </li>
    <li>
        The user want to listen or download the file, the web app call the Valet Key api to get a SaS token to the private blob storage.
    </li>
</ol>