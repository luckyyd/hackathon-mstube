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
from channel9.items import Channel9Item


class Channel9Spider(scrapy.Spider):
    name = 'channel9'
    allowed_domains = ["channel9.msdn.com"]
    start_urls = ["https://channel9.msdn.com/Browse/Shows/"]

    def __init__(self):
        self.counter = 0
        self.main_url = "https://channel9.msdn.com"

    def parse(self, response):
        shows_url = response.xpath(
            '//ul[@class="entries"]/li/div[@class="entry-meta"]/a/@href').extract()
        for show_url in shows_url:
            url = self.main_url + show_url
            self.log('Found show url: %s' % url)
            yield scrapy.Request(url, callback=self.parse_video)
        try:
            next_page_url = self.main_url + \
                response.xpath(
                    '//ul[@class="paging"]/li[@class="next"]/a/@href').extract()[0]
            self.log("Crawling next shows page: %s" % next_page_url)
            yield scrapy.Request(next_page_url, callback=self.parse)
        except Exception:
            self.log("Crawled all shows url.")

    def parse_video(self, response):
        data = response.xpath('//ul[@class="entries"]//li')
        try:
            topic = response.xpath(
                '//div[@class="area-header item-header"]/h1/text()').extract()[0].strip()
        except Exception as err:
            self.log('Topic extract error with ' + str(response))
            return
        for entry in data:
            try:
                # Get video title
                title = entry.xpath(
                    'div[@class="entry-meta"]/a/text()').extract()[0].strip()
                title = re.sub('\u00a0', ' ', title)
                # Get video brief description
                try:
                    video_description = entry.xpath(
                        '//div[@class="description"]/text()').extract()[0]
                    video_description = re.sub(r'\r', '', video_description)
                    video_description = re.sub(r'\n', '', video_description)
                    video_description = re.sub(r'\t', '', video_description)
                except Exception as err:
                    self.log('Video description error with ' + str(response))
                # Get video image source and video url
                image_src = entry.xpath(
                    'div[@class="entry-image"]/a/img/@src').extract()[0]
                url = entry.xpath(
                    'div[@class="entry-image"]/a/@href').extract()[0]
                url = self.main_url + url

                req = scrapy.Request(url, callback=self.parse_video_detail)
                req.meta['topic'] = topic
                req.meta['title'] = title
                req.meta['description'] = video_description
                req.meta['image_src'] = image_src
                req.meta['url'] = url
                yield req
            except Exception as err:
                self.log('item error: ' + str(err))
        try:
            next_page_url = self.main_url + \
                response.xpath(
                    '//ul[@class="paging"]/li[@class="next"]/a/@href').extract()[0]
            self.log("Crawling next page: %s" % next_page_url)
            yield scrapy.Request(next_page_url, callback=self.parse_video)
        except Exception:
            self.log("Crawl done with topic: " + topic)

    def parse_video_detail(self, response):
        try:
            tags = response.xpath(
                '//div[@id="entry-tags"]/ul/li/a/text()').extract()
        except Exception:
            tags = []
        video_src = response.xpath('//a[@class="video"]/@href').extract()[0]
        str_views = response.xpath(
            '//span[@class="count"]/text()').extract()[0]
        views = int(''.join(str_views.split(',')))
        try:
            upload_date = response.xpath(
                '//script[@type="application/ld+json"]').re('"uploadDate":"(.*?)T')[0]
        except Exception:
            self.log('This video is too early.')
            return
        avg_rating = float(response.xpath(
            '//p[@class="avg-rating"]').re('([\d.]+)')[0])
        full_description = response.xpath(
            '//div[@id="entry-body"]').extract()[0]
        """Generate an item"""
        item = Channel9Item()
        item['title'] = response.meta['title']
        item['topic'] = response.meta['topic']
        item['url'] = response.meta['url']
        item['description'] = response.meta['description']
        item['image_src'] = response.meta['image_src']
        item['video_src'] = video_src
        item['tags'] = tags
        item['views'] = views
        item['category'] = 'video'
        item['source'] = 'channel9'
        item['upload_date'] = upload_date
        item['avg_rating'] = avg_rating
        item['full_description'] = full_description
        self.counter += 1
        item['item_id'] = self.counter
        yield item
