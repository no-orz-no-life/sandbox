import re
import sys

r = re.compile("^(.+\/)(\d{8}-\d{6}-)(\d{8}-\d{6}-)(.+\..{3})$")
for line in sys.stdin:
    m = r.match(line)
    if m:
        print(f"mv {m.group(0)} {m.group(1)}{m.group(3)}{m.group(4)}")
