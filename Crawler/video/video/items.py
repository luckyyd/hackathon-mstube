# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy


class VideoItem(scrapy.Item):
    # define the fields for your item here like:
    title = scrapy.Field()
    topic = scrapy.Field()
    url = scrapy.Field()
    video_src = scrapy.Field()
    video_description = scrapy.Field()
    image_src = scrapy.Field()
    # crawled_time = scrapy.Field()
    video_id = scrapy.Field()
