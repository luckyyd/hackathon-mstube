**Here is the backend of the project.**

## Introduction

The project is to transfer data from several data storage sources and provides important APIs for the client.

### APIs

`GET` methods:

- `UserId` (get user id by device id) and some others.
- `Candidates` (main recommend)
- `ListLatest` (list latest items)
- `SearchTitle` (search for items by title)
- `SearchTopic` (search for items by topic)

`POST` methods:

- `Preference` (give the user's preference of one item)

### Data Storage

The project need to connect to Redis and SQL Server, Azure Machine Learning Service. They are all deployed on Azure.

- `Redis` is to save the data of user id, recommendation history, and last clicked item.
- `Microsoft SQL Server` holds the video detail data. It was crawled by crawlers.
- `Azure Machine Learning Service` provides two APIs. One is for the most recommend items for one user (using Collaborative Filtering Algorithm), the other is for the most related item of one specific item.
- `blob` is used to conserve preference data, which is used to retrain the recommendation model.

## Usage

Load the project by `.sln` solution file.

The project was published at `http://mstubedotnet.azurewebsites.net/`.

Client will send `GET` or `POST` request to the opposite urls.

The API web endpoint has the form of `https://mstubedotnet.azuresebsites.net/api/***?x=...&y=...`, here `***` is the method name, and `…` is the parameters.

The response data is in 'json' format.