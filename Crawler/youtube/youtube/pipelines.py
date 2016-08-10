# -*- coding: utf-8 -*-

# Define your item pipelines here
#
# Don't forget to add your pipeline to the ITEM_PIPELINES setting
# See: http://doc.scrapy.org/en/latest/topics/item-pipeline.html
import pymssql


class SQLServerPipeline(object):

    def __init__(self):
        server = "mstube.database.windows.net"
        user = "mstube@mstube.database.windows.net"
        password = ""  # Fill it!
        database = "mstube-dotnet-db"
        self.conn = pymssql.connect(
            server=server, user=user, password=password, database=database)
        self.cursor = self.conn.cursor()

    def process_item(self, item, spider):
        try:
            self.cursor.execute("INSERT INTO TestPy(item_id, title) VALUES (%s, %s)",
                                (item['item_id'], item['title']))
            self.conn.commit()
        except pymssql.Error as e:
            print("Error with pymssql: " + str(e))
