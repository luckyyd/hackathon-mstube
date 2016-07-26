# This script is to filter specific field of the json file
import json
from pprint import pprint
with open("video/items.json") as data_file:
    data = json.load(data_file)

output1 = []
output2 = []
for item in data:
    item_id = int(item['item_id'])
    tags = item['tags']
    title = item['title']
    topic = item['topic']
    output1.append({"item_id": item_id, "tags": tags})
    output2.append({"item_id": item_id, "tags": tags, "title": title, "topic": topic})
# pprint(output1)
# print('\n\n\n')
# pprint(output2)
with open('output1.json', 'w') as output_file1:
    json.dump(output1, output_file1)
with open('output2.json', 'w') as output_file2:
    json.dump(output2, output_file2)
