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
    start_urls = ["https://channel9.msdn.com/Browse/Shows/"]

    def __init__(self):
        self.counter = 0
        self.main_url = "https://channel9.msdn.com"

    def parse(self, response):
        shows_url = response.xpath('//ul[@class="entries"]/li/div[@class="entry-meta"]/a/@href').extract()
        for show_url in shows_url:
            url = self.main_url + show_url
            self.log('Found show url: %s' % url)
            yield scrapy.Request(url, callback=self.parse_video)
        try:
            next_page_url = main_url + response.xpath('//ul[@class="paging"]/li[@class="next"]/a/@href').extract()[0]
            self.log("Crawling next shows page: %s" % next_page_url)
            yield scrapy.Request(next_page_url, callback=self.parse)
        except Exception:
            self.log("Crawled all shows url.")

    def parse_video(self, response):
        data = response.xpath('//ul[@class="entries"]//li')
        unichange = re.compile('\u00a0')
        pattern_video_src = re.compile(r'<a class="video".*?href="(.*\.mp4)"')
        pattern_tags = re.compile(r'<a href="/Tags.*?">(.*)</a>')
        pattern_views = re.compile(r'<span class="count">(\d*)</span>')
        pattern_upload_date = re.compile(r'"uploadDate":"([\d-]*)T')
        pattern_avg_rating = re.compile(r'<p class="avg-rating">.*?([\d.]+)</p>')
        topic = response.xpath('//div[@class="area-header item-header"]/h1/text()').extract()[0].strip()
        for entry in data:
            try:
                # Get video title
                title = entry.xpath('div[@class="entry-meta"]/a/text()').extract()[0].strip()
                title = unichange.sub(' ', title)
                # Get video brief description
                try:
                    video_description = entry.xpath('//div[@class="description"]/text()').extract()[0]
                    video_description = re.sub('\u00a0', ' ', video_description)
                    video_description = re.sub('\u2026', ' ', video_description)
                    video_description = re.sub(r'\r', '', video_description)
                    video_description = re.sub(r'\n', '', video_description)
                    video_description = re.sub(r'\t', '', video_description)
                except Exception as err:
                    print(err)
                # Get video image source and video url
                image_src = entry.xpath('div[@class="entry-image"]/a/img/@src').extract()[0]
                url = entry.xpath('div[@class="entry-image"]/a/@href').extract()[0]
                url = self.main_url + url

                """Turn to video detail page"""
                try:
                    subpage = requests.get(url, timeout=5.0)
                    subpage_content = subpage.content.decode('utf-8')
                except Exception as err:
                    # Jump to next item
                    continue
                video_src = pattern_video_src.findall(subpage_content)[0]
                tags = pattern_tags.findall(subpage_content)
                views = int(pattern_views.findall(subpage_content)[0])
                upload_date = pattern_upload_date.findall(subpage_content)[0]
                avg_rating = float(pattern_avg_rating.findall(subpage_content)[0])
                full_description = ''
                """Generate an item"""
                item = VideoItem()
                item['title'] = title
                item['topic'] = topic
                item['url'] = url
                item['video_src'] = video_src
                item['description'] = video_description
                item['image_src'] = image_src
                item['tags'] = tags
                item['views'] = views
                item['upload_date'] = upload_date
                item['avg_rating'] = avg_rating
                item['full_description'] = full_description
                self.counter += 1
                item['item_id'] = self.counter
                yield item
            except Exception as err:
                print(err)
        # try:
        #     next_page_url = main_url + response.xpath('//ul[@class="paging"]/li[@class="next"]/a/@href').extract()[0]
        #     self.log("Crawling next page: %s" % next_page_url)
        #     yield scrapy.Request(next_page_url, callback=self.parse)
        # except Exception:
        #     self.log("Crawling done.")
