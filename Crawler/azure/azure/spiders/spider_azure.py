# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy
import time
from azure.items import AzureItem

from selenium import webdriver

class AzureSpider(scrapy.Spider):
    name = 'azure'
    allowed_domains = ["channel9.msdn.com"]
    start_urls = ["https://channel9.msdn.com/Browse/Shows"]
    

    def __init__(self):
      scrapy.Spider.__init__(self)
      self.driver = webdriver.Firefox()

    def __del__(self):
      self.driver.close()

    def parse(self, response):
      self.driver.get("https://channel9.msdn.com/Browse/Shows")
      #self.browser.get(response.url)
      time.sleep(5)
      counter = 1
      
      while True:
      
        hxs = scrapy.Selector(text = self.driver.page_source)

        for info in hxs.xpath('//ul[@class="entries"]/li'):
          item = AzureItem()
          item['Id'] = "[" + str(counter) + "]"
          counter += 1
          item['title'] = info.xpath('div[@class="entry-image"]/img/@alt').extract()
          item['link'] = info.xpath('div[@class="entry-meta"]/a/@href').extract()
          item['imagelink'] = info.xpath('div[@class="entry-image"]/img/@src').extract()
          item['description'] = info.xpath('div[@class="entry-meta"]/div[@class="description"]/text()').extract()
          yield item

        next = self.driver.find_element_by_xpath('//li[@class="next"]/a')
        #print next.text
        time.sleep(2)

        try:
          next.click()

        except Exception as err:
          print(err)
          break
 



