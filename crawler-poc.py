# POC of bacterio.net crawler
import requests
import re
import csv

test_term = "Desulfovibrio"
site_url = "www.bacterio.net"
cx = "014660791402971582651:tcuxzzjmugs"
encoding = "UTF-8"
key = "AIzaSyA9PNaz5VtB-yVUxsKSHPWOoTdv3I0899Y"
baseUrl = "https://www.googleapis.com/customsearch/v1"

# 8 columns for data + keywords
dataSize = 8


# https://cse.google.com/cse?q=test-search&sa=Search&cx=014660791402971582651%3Atcuxzzjmugs&ie=UTF-8&siteurl=www.bacterio.net%2Fsediminibacterium.html&ref=&ss=&siteurl=%s&ref=&ss=

class Parser:
	regex = re.compile(";")

	empty_item_regex = re.compile(" [pcofgs]__[\n]?$")
	item_regex = re.compile("[kpcofgs]__([a-zA-Z]+)")

	@staticmethod
	def parse_line(line):
		"""
		Splits the line.

		:param str line:
		:return: List representation of line.
		"""
		return Parser.regex.split(line)

	# parsed = []
	# for item in items:
	# 	if not is_empty_item(item):
	# 		m = item_regex.search(item)
	# 		if m is not None:
	# 			parsed.append(m[1])
	#
	# return parsed

	@staticmethod
	def is_empty_item(item):
		return len(item) == 0 or Parser.empty_item_regex.match(item)

	@staticmethod
	def parse_term_from_line_item(item):
		m = Parser.item_regex.search(item)
		if m is not None:
			return m[1]
		else:
			return None


def load_file(filename):
	return open(filename, 'r')


def search_term(term):
	"""
	This method uses paid API.

	:param str term: Term to search for.
	:return: Link to bacterio page with given term.
	"""

	q = term

	payload = {
		'key': key,
		'q': q,
		'cx': cx
	}
	resp = requests.get(baseUrl, params=payload)
	print(f'Searching {term}: {resp.status_code}')
	data = resp.json()
	if "items" in data:
		return data["items"][0]["link"]
	else:
		return None


def download_page(link, term):
	"""
	Downloads page given by link and saves it to file
	named html/<term>.html

	:param str link: Link to page to download.
	:param str term: Term used to name the page.
	:return: Nothing
	"""

	resp = requests.get(link)
	print(f'Parsing page {link}: {resp.status_code}')
	with open('html/' + term + '.html', 'w') as f:
		print(str(bytes(resp.text, "utf-8")), file=f)


def search_for_keywords(term, keywords):
	"""
	Performs search for keywords in a page downloaded into
	file called html/<term>.html.

	:param str term: Term.
	:param dict keywords: Keywords to look for in the file.
	:return: Search result (list).
	"""
	res = []
	content = ''
	with open('html/' + term + '.html', 'r') as f:
		content = f.read()

	for keyword in keywords:

		# search for negative
		is_negative = False
		for k_negative in keywords[keyword]:
			if k_negative in content:
				res.append('0')
				is_negative = True
				break

		# no negative, try positive
		if not is_negative:
			if keyword in content:
				res.append('1')
			else:
				# nothing
				res.append('-')

	return res


def save_search_res(term, res, keywords):
	with open('res/' + term + '.csv', 'w', newline='') as res_file:
		writer = csv.writer(res_file, quoting=csv.QUOTE_ALL)
		writer.writerow(keywords)
		writer.writerow(res)


def load_keywords():
	"""
	Loads keywords from file.
	Expected file structure:

	keyw 1;keyw 1 neg 1, keyw 1 neg 2; ...
	keyw 2;keyw 2 neg 1, keyw 2 neg 2; ...
	...

	:return: Dict with keywords.
	"""

	keywords = {}
	with open('keywords.csv') as k_file:
		reader = csv.reader(k_file, delimiter=';', quoting=csv.QUOTE_MINIMAL)
		for line in reader:
			keyword = None
			for item in line:
				if keyword is None:
					# keyword on the 1st position on line
					keyword = item
					keywords[keyword] = []
				else:
					# keyword negatives
					keywords[keyword].append(item)

	return keywords


def save_search_result(source_line, search_res):
	"""
	Appends one line containing the search results into out file.

	:param list source_line: Line from source file.
	:param list search_res: Search results.
	"""

	if len(search_res) == 0:
		return

	res_line = []
	res_line.extend(source_line)
	res_line.extend(search_res)

	with open("result.csv", "a", newline='') as out_file:
		writer = csv.writer(out_file, delimiter=';')
		writer.writerow(res_line)


def search_for_term(term, keywords):
	"""
	Performs search for given term.

	:param str term: Term to search for.
	:param keywords: Keywords to search for in found page.
	:return: List representing occurrence of keywords.
	"""

	link = search_term(term)
	if link is None:
		print(f'No link for {term} found')
		return []
	download_page(link, term)
	return search_for_keywords(term, keywords)


def process_source_line(source_line):
	"""
	Splits line from source file and returns it as a list of items.

	:param str source_line: One line from source file.
	:return: Line split to items.
	"""

	return Parser.parse_line(source_line.rstrip())


def pick_term_from_line(source_line):
	"""
	Picks term to search for from line from source file.

	:param list source_line:
	:return: Term to search for (or None).
	"""

	for item in reversed(source_line):
		if not Parser.is_empty_item(item):
			return Parser.parse_term_from_line_item(item)

	return None


def prepare_out_file(keywords):
	"""
	Clears out file (if it exists) and adds header line.

	:param dict keywords: Keywords, used to prepare headers.
	"""

	# CID_1372640;2.0;k__Bacteria; p__Proteobacteria; c__Alphaproteobacteria; o__Sphingomonadales; f__Sphingomonadaceae; g__Sphingomonas; s__
	header_line = ['OTU ID', 'S1', 'k', 'p', 'c', 'o', 'f', 'g', 's']
	for keyword in keywords:
		header_line.append(keyword)

	with open("result.csv", "w", newline='') as out_file:
		writer = csv.writer(out_file, delimiter=';')
		writer.writerow(header_line)


def main():
	source_file = load_file('S1b-reduced.csv')
	search_map = {}
	keywords = load_keywords()

	# prepare header line
	prepare_out_file(keywords)

	for line in source_file:
		if line[0] != '#':

			# get line as list
			line_list = process_source_line(line)

			# pick term to search for from line
			term = pick_term_from_line(line_list)

			# either get results from search_map (cache)
			# or find it on bacterio
			if term not in search_map:
				search_res = search_for_term(term, keywords)
				search_map[term] = search_res
			else:
				search_res = search_map[term]

			# save results
			save_search_result(line_list, search_res)

	source_file.close()

	print('Done')


#
# Script body
#
main()
