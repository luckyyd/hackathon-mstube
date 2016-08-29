# -*- coding: utf-8 -*-

# Define your item pipelines here
#
# Don't forget to add your pipeline to the ITEM_PIPELINES setting
# See: http://doc.scrapy.org/en/latest/topics/item-pipeline.html

import pymssql
import SQLsetting


class SQLServerPipeline(object):

    def __init__(self):
        self.conn = pymssql.connect(server=SQLsetting.server, user=SQLsetting.user, database=SQLsetting.database,
                                    password=SQLsetting.password, port=1433, tds_version="8.0")
        self.cursor = self.conn.cursor()

    def process_item(self, item, spider):
        try:
            self.cursor.execute(
                "IF NOT EXISTS (SELECT * FROM ItemTest Where url = %s) \
                 INSERT INTO ItemTest(title, video_src, image_src, url, \
                views, category, posted_time, description, full_description, source) \
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
                (item['url'], item['title'], item['video_src'], item['image_src'],
                 item['url'], item['views'], item[
                     'category'], item['upload_date'],
                 item['description'], item['full_description'], item['source']))
            self.conn.commit()
        except pymssql.Error as e:
            print("Error with pymssql: " + str(e))
