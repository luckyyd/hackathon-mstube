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
                "IF NOT EXISTS (SELECT * FROM Item Where url = %s) \
                 INSERT INTO Item(title, topic, video_src, image_src, url, \
                views, category, posted_time, quality, description, full_description, source) \
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
                (item['url'], item['title'], item['topic'], item['video_src'], item['image_src'],
                 item['url'], item['views'], item['category'],
                 item['upload_date'], item['avg_rating'],
                 item['description'], item['full_description'], item['source']))
            self.conn.commit()
        except pymssql.Error as e:
            print("Error with pymssql: " + str(e))
