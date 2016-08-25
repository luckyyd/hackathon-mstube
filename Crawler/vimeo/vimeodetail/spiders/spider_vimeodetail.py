# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy
import time
import re
from vimeodetail.items import VimeodetailItem
from vimeodetail.PageDataLink import PageDataLink


class VimeodetailSpider(scrapy.Spider):
    name = 'vimeodetail'
    allowed_domains = ["vimeo.com"]
    start_urls = [(word) for word in PageDataLink().page]

    def __init__(self):
        scrapy.Spider.__init__(self)
        self.counter = 6000

    def __del__(self):
        self.driver.close()

    def parse(self, response):
        hxs = response
        item = VimeodetailItem()
        item['url'] = hxs.xpath(
            '//meta[@property="og:url"]/@content').extract()[0]
        item['video_src'] = hxs.xpath(
            '//meta[@property="og:url"]/@content').extract()[0]
        item['image_src'] = hxs.xpath(
            '//meta[@name="twitter:image"]/@content').extract()[0]
        item['title'] = hxs.xpath(
            '//meta[@name="twitter:title"]/@content').extract()[0]
        item['description'] = hxs.xpath(
            '//meta[@name="description"]/@content').extract()[0]
        item['full_description'] = hxs.xpath(
            '//meta[@name="description"]/@content').extract()[0]
        try:
            item['topic'] = hxs.xpath(
                '//meta[@property="video:tag"]/@content').extract()[0]
        except:
            item['topic'] = hxs.xpath(
                '//meta[@property="article:tag"]/@content').extract()[0]
        item['posted_time'] = hxs.xpath(
            '//span[@class="clip_info-time"]/time/@datetime').re(r'([\d-]*)T')[0]
        try:
            item['views'] = int(hxs.xpath(
                '//script[@type="application/ld+json"]/text()').re(r'"interactionCount":(\d*)')[0])
        except:
            item['views'] = int(100)
        item['category'] = str("video")
        item['source'] = str("vimeo")
        item['quality'] = int(4)
        yield item
