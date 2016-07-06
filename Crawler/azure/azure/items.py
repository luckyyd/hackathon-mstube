# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy

class AzureItem(scrapy.Item):
    Id = scrapy.Field()
    title = scrapy.Field()
    link = scrapy.Field()
    imagelink = scrapy.Field()
    description = scrapy.Field()
