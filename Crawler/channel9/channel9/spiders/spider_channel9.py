# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy
import time
import json
import re
from channel9.items import ChannelItem


class ChannelSpider(scrapy.Spider):
    name = 'channel9'
    allowed_domains = ["channel9.msdn.com"]
    start_urls = ["https://channel9.msdn.com/Azure/"]

    def parse(self, response):
        items = []
        main_url = "https://channel9.msdn.com"
        data = response.xpath('//ul[@class="areas"]//li')
        # unichange1 = re.compile('\u00a0')
        unichange = re.compile('\u00a0')
        counter = 0
        for entry in data:
            try:
                image = entry.xpath('div[@class="entry-image"]/img/@src').extract()[0]
                title = entry.xpath('div[@class="entry-meta"]/a/text()').extract()[0]
                title = unichange.sub(' ', title)
                url = main_url + entry.xpath('div[@class="entry-meta"]/a/@href').extract()[0]
                try:
                    description = entry.xpath('.//div[@class="description"]/text()').extract()[0]
                    description = unichange.sub(' ', description)
                    description = re.sub('\u2026', ' ', description)
                except Exception:
                    description = ''
                item = ChannelItem()
                item['title'] = title
                item['url'] = url
                item['image_src'] = image
                # item['crawled_time'] = time.strftime('%Y-%m-%d %H:%M', time.localtime())
                item['description'] = description
                counter += 1
                item['id'] = counter
                # print(title, url)
                yield item
            except Exception as err:
                print(err)
