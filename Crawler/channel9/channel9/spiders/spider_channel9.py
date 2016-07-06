# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy
import time
import json
# from selenium.webdriver.common.keys import Keys
# from selenium import webdriver
# from selenium.webdriver.common.action_chains import ActionChains
from channel9.items import ChannelItem
# import sys
# reload(sys)
# sys.setdefaultencoding('utf-8')

# def cleanse(alist):
# return alist[0].strip().encode('utf-8').replace('"', '“').replace('\n',
# '').replace('\t', '    ').replace('\\', '“') if alist else u''


class ChannelSpider(scrapy.Spider):
    name = 'channel9'
    allowed_domains = ["channel9.msdn.com"]
    start_urls = ["https://channel9.msdn.com/Azure/"]

    def parse(self, response):
        items = []
        data = response.xpath('//ul[@class="areas"]//li')
        counter = 0
        for entry in data:
            try:
                image = entry.xpath('div[@class="entry-image"]/img/@src').extract()[0]
                # data.xpath('div[@class="entry-meta"]/a')
                title = entry.xpath('div[@class="entry-meta"]/a/text()').extract()[0]
                url = entry.xpath('div[@class="entry-meta"]/a/@href').extract()[0]
                item = ChannelItem()
                item['title'] = title
                item['url'] = url
                item['image_src'] = image
                item['crawled_time'] = time.strftime('%Y-%m-%d %H:%M', time.localtime())
                counter += 1
                item['id'] = counter
                # print(title, url)
                items.append(item)
            except Exception as err:
                print(err)
        return items
