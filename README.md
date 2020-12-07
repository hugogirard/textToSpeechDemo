# What this demo about

This demo expose an Azure Architecture using Azure Cognitive Services to convert text to audio.  The user enter a text in a Blazor Server web app.  A job is created and an azure function is triggered, the function will call Azure Cognitive Services to detect the text language (currently french and english are supported).

When the text is detected another function will be triggered and will call again Azure Cognitive Services to conver the text to audio file and save it in a blob.

# Architecture Diagram

