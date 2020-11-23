(function (selector, wordsRange, baseColor, gradColors) {
        function level(s) {
            if(s.length === 0) return 0
            return Array.from(s).map(function (c) {
                if(c === '<') return 1
                if(c === '>') return -1
                return 0
            }).reduce((acc, v) => acc+v)
        }
    
        let getStyle = (function () {
            let gen = function* () {
                var ptr = 0
                while(1) {
                    yield `${baseColor}, ${gradColors[ptr]}`
                    yield `${gradColors[ptr]}, ${baseColor}`
                    ptr += 1
                    if(ptr >= gradColors.length) {
                        ptr = 0
                    }
                }
            }()
            return function () {
                return "background: linear-gradient(90deg, " + gen.next().value + "); -webkit-background-clip: text; -webkit-text-fill-color: transparent;"
            }
        })()
    
        document.querySelectorAll(selector).forEach(function (p) {
            var l = 0
            var count = 0
            p.innerHTML = '<span style="' + getStyle() + '">' + (p.innerHTML.split(" ").map(function (word) {
                l += level(word)
                if(l === 0) {
                    count += 1
                }
                if(count === wordsRange) {
                    count = 0
                    return '</span><span style="' + getStyle() + '">' + word
                } else {
                    return word
                }
            }).join(" ")) + "</span>"
        })
    })("p", 16, "black", ["blue", "red"])