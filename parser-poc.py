# POC of a parser of source data files for bacterio

import re
import csv

regex = re.compile(";")

empty_item_regex = re.compile(" [pcofgs]__[\n]?$")
item_regex = re.compile("[kpcofgs]__([a-zA-Z]+)")

def parse_line(line):
	items = regex.split(line)
	parsed = []
	for item in items:
		if not is_empty_item(item):
			m = item_regex.search(item)
			if m is not None:
				parsed.append(m[1])
	
	return parsed

def is_empty_item(item):
	return len(item) == 0 or empty_item_regex.match(item)

	
def load_file(filename):
	return open(filename, 'r')
	
def main():
	source_file = load_file("S1b-reduced.csv")
	out_file = open("parsed.csv", "w", newline='')
	out_w = csv.writer(out_file, quoting=csv.QUOTE_ALL)
	for line in source_file:
		if line[0] != '#':
			out_w.writerow(parse_line(line))
			
	source_file.close()
	out_file.close()
	
main()
		