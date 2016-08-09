# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy


class VimeodetailItem(scrapy.Item):
    item_id = scrapy.Field()
    url = scrapy.Field()
    video_src = scrapy.Field()
    image_src = scrapy.Field()
    title = scrapy.Field()
    description = scrapy.Field()
    full_description = scrapy.Field()
    topic = scrapy.Field()	
    posted_time = scrapy.Field()
    views = scrapy.Field()
    category = scrapy.Field()
    source = scrapy.Field()
    
