from wordfreq import zipf_frequency


with open('dictionary.txt') as wordlist:
    with open('weighted_dictionary.txt', 'w') as outfile:
        lines = []

        for word in wordlist.readlines():
            word = word.strip()
            popularity = zipf_frequency(word.lower(), 'en')
            lines.append(f'{word} {popularity}\n')
        
        outfile.writelines(lines)