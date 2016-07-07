# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy
import time
import json
import requests
import re
from video.items import VideoItem


class VideoSpider(scrapy.Spider):
    name = 'video'
    allowed_domains = ["channel9.msdn.com"]
    start_urls = ["https://channel9.msdn.com/Shows/Cloud+Cover/"]

    def __init__(self):
        self.video_src = ''

    # def parse_content(self, response):
    #     self.video_src = response.xpath('//div[@class="playerContainer"]/a[@class="video"]/@href').extract()[0]
    #     print(self.video_src)

    def parse(self, response):
        items = []
        main_url = "https://channel9.msdn.com"
        data = response.xpath('//ul[@class="entries"]//li')
        self.counter = 0
        for entry in data:
            try:
                title = entry.xpath('div[@class="entry-meta"]/a/text()').extract()[0]
                topic = 'Microsoft Azure Cloud Cover Show'
                video_description = entry.xpath('//div[@class="description"]/text()').extract()[0]
                image_src = entry.xpath('div[@class="entry-image"]/a/img/@src').extract()[0]
                url = entry.xpath('div[@class="entry-image"]/a/@href').extract()[0]
                url = main_url + url
                # Here to get video src by subpage.
                subpage = requests.get(url)
                subpage_content = subpage.content.decode('utf-8')
                pattern = re.compile(r'<a class="video".*?href="(.*?\.mp4)"')
                result = pattern.findall(subpage_content)
                video_src = result[0]
                # Generate an item
                item = VideoItem()
                item['title'] = title
                item['topic'] = topic
                item['url'] = url
                item['video_src'] = video_src
                item['video_description'] = video_description
                item['image_src'] = image_src
                # item['crawled_time'] = time.strftime('%Y-%m-%d %H:%M', time.localtime())
                self.counter += 1
                item['video_id'] = self.counter
                yield item
            except Exception as err:
                print(err)
        try:
            next_page_url = main_url + response.xpath('//ul[@class="paging"]/li[@class="next"]/a/@href').extract()[0]
            print("Crawling next page: %s" % next_page_url)
            yield scrapy.Request(next_page_url, callback=self.parse)
        except Exception:
            print("Crawling done.")
