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

#from selenium import webdriver

class VimeodetailSpider(scrapy.Spider):
    name = 'vimeodetail'
    allowed_domains = ["vimeo.com"]
    start_urls = [(word) for word in PageDataLink().page]

    def __init__(self):
      scrapy.Spider.__init__(self)
      #self.driver = webdriver.PhantomJS()
      self.counter = 6000

    def __del__(self):
      self.driver.close()

    def parse(self, response):
      	#self.driver.get(page)
      	#hxs = scrapy.Selector(text = self.driver.page_source)
	hxs = response
		
	#for info in hxs.xpath('//div[@class="iris_grid-content"]'):
	item = VimeodetailItem()
	item['item_id'] = str(self.counter)
	self.counter += 1
	item['url'] = hxs.xpath('//meta[@property="og:url"]/@content').extract()
	item['video_src'] = hxs.xpath('//meta[@property="og:url"]/@content').extract()
	item['image_src'] = hxs.xpath('//meta[@name="twitter:image"]/@content').extract()
	item['title'] = hxs.xpath('//meta[@name="twitter:title"]/@content').extract()
	item['description'] = hxs.xpath('//meta[@name="description"]/@content').extract()
	item['full_description'] = hxs.xpath('//meta[@name="description"]/@content').extract()
	item['topic'] = str("Microsoft")
	item['posted_time'] = hxs.xpath('//span[@class="clip_info-time"]/time/@datetime').extract()
	item['views'] = hxs.xpath('//script[@type="application/ld+json"]/text()').re(r'"interactionCount":(\d*)')
	item['category'] = str("video")
	item['source'] = str("vimeo")
	yield item





