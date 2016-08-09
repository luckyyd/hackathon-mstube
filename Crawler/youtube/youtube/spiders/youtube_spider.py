import re
import scrapy
import time
from youtube.items import YoutubeItem


class YoutubeSpider(scrapy.Spider):
    name = 'youtube'
    allowed_domains = ["youtube.com"]
    start_urls = [
        "https://www.youtube.com/user/Microsoft/videos?sort=dd&flow=list"]

    def __init__(self):
        self.counter = 0
        self.main_url = 'https://www.youtube.com/'

    def parse(self, response):
        entries = response.xpath(
            '//li[contains(@class,"feed-item-container")]')
        for entry in entries:
            try:
                title = entry.xpath('.//h3/a/text()').extract()[0]
                url = self.main_url + entry.xpath('.//h3/a/@href').extract()[0]
                image_src = entry.xpath('.//img/@data-thumb').extract()[0]
                description = entry.xpath('.//div[contains(@class, "yt-lockup-description")]/text()').extract()[0]
                description = description.strip('\n').strip()
                req = scrapy.Request(url, callback=self.parse_video)
                req.meta['title'] = title
                req.meta['url'] = url
                req.meta['image_src'] = image_src
                req.meta['description'] =description
                yield req
            except Exception as err:
                self.log(str(err))

    def parse_video(self, response):
        try:
            views_text = response.xpath('//div[@class="watch-view-count"]/text()').extract()[0]
            views = int(''.join(re.findall('[\d,]+',views_text)[0].split(',')))
        except Exception as err:
            views = 0
        upload_date = response.xpath('//meta[@itemprop="datePublished"]/@content').extract()[0]
        full_description = response.xpath('//p[@id="eow-description"]/text()').extract()[0]
        item = YoutubeItem()
        item['title'] = response.meta['title']
        item['url'] = response.meta['url']
        item['image_src'] = response.meta['image_src']
        item['video_src'] = response.meta['url']
        item['description'] = response.meta['description']
        item['full_description'] = full_description
        item['upload_date'] = upload_date
        item['views'] = views
        item['category'] ='video'
        self.counter +=1
        item['item_id'] = self.counter
        yield item
