# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy


class YoutubeItem(scrapy.Item):
    # define the fields for your item here like:
    item_id = scrapy.Field()
    title = scrapy.Field()
    tags = scrapy.Field()
    topic = scrapy.Field()
    video_src = scrapy.Field()
    image_src = scrapy.Field()
    url = scrapy.Field()
    views = scrapy.Field()
    category = scrapy.Field()
    upload_date = scrapy.Field()
    avg_rating = scrapy.Field()
    description = scrapy.Field()
    full_description = scrapy.Field()
    source = scrapy.Field()
