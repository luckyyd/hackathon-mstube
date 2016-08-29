# Crawler

Here is the crawlers of the project.

- `channel9` is for videos from channel9
- `vimeo` is for videos from vimeo
- `youtube` is for videos from youtube

### Environment

You need to install these two packages first.

```shell
pip install scrapy  # Scrapy framework
pip install pymssql # Microsoft SQL Server
```

Or as recommend, run the project under the docker container: [docker-scrapy](https://github.com/irmowan/docker-scrapy)

### Usage

Run `scrapy crawl 'project'` under the opposite crawler folder. Here, `'project'` should be replaced by `channel9`/`vimeo`/`youtube`.

Notice that the items will be written into Azure SQL Server.

All the `items.json` files are the example output, you could get it by run `scrapy crawl 'project' -o items.json`.

Folder `tools` is just for generating data, ignore it.

---

Author: Yimu

Date: 2016.7.6
