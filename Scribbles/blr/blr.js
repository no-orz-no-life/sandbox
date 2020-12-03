((selector, countRange) => {
        const prefix = "prmnsblr"
    
        function makeProcessor() {
            var state = 0
            var level = 0
            return (c) => {
                let beforeState = state
                let beforeLevel = level
                if (state === 0) {
                    if (c === '<') { level += 1; state = 1 }
                } else if (state === 1) {
                    if (c === '/') { state = 2 }
                    else { state = 3 }
                } else if (state === 2) {
                    if (c === '>') { level -= 2; state = 0 }
                } else if (state == 3) {
                    if (c === '>') { state = 0 }
                }
                //console.log(`input: ${c}, level: ${beforeLevel}->${level}, state: ${beforeState}->${state}`)
                return level
            }
        }
        function insertStyle(rule) {
            var sheet = document.styleSheets[document.styleSheets.length - 1]
            if (sheet === undefined) {
                let element = document.createElement("style")
                document.head.append(element)
                sheet = element.sheet
            }
            sheet.insertRule(rule, sheet.cssRules.length)
        }
        function isDupulicateApplication() {
            return (document.querySelector(`.${prefix}0`) !== null)
        }
        function needsBrighter() {
            var result = false
            let e = document.querySelector(selector)
            if (e !== null) {
                let color = window.getComputedStyle(e).color
                if (color.startsWith("rgb")) {
                    let colors = color.split(")")[0].split("(")[1].split(", ").map(s => parseInt(s, 10))
                    if ((((colors[0] * 299) + (colors[1] * 587) + (colors[2] * 114)) / 1000) > 128) { result = true }
                }
            }
            if (isDupulicateApplication()) { result = !result }
            return result
        }
        function cleanUpOldStyles() {
            Array.from(document.styleSheets).filter(
                (styleSheet) => !styleSheet.href || styleSheet.href.startsWith(window.location.origin)
            ).forEach((sheet) => {
                Array.from(sheet.cssRules).map((v, idx) => [v, idx]).filter((v) => v[0].selectorText !== undefined && v[0].selectorText.startsWith(`span.${prefix}`)).map((v) => v[1]).sort((a, b) => b - a).forEach((v) => {
                    try {
                        document.styleSheets[0].deleteRule(v)
                    } catch (e) { } // https://chromium.googlesource.com/chromium/src/+/a4ebe08c91e29140e700c7bae9b94f27a786d1ca
                })
            })
        }
    
        var baseColor = "black"
        var gradColors = ["blue", "red"]
    
        if (needsBrighter()) {
            baseColor = "white"
            gradColors = ["#14ff00", "#e32d4d"]
        }
    
        function prepareStyle() {
            var ptr = -1
            function makeStyle(c1, c2) {
                ptr += 1
                return `span.${prefix}${ptr} { background: linear-gradient(90deg, ${c1}, ${c2}) !important; -webkit-background-clip: text !important; -webkit-text-fill-color: transparent !important; }`
            }
            gradColors.forEach((color) => {
                insertStyle(makeStyle(baseColor, color))
                insertStyle(makeStyle(color, baseColor))
            })
            return ptr
        }
    
        if (isDupulicateApplication()) {
            cleanUpOldStyles()
            prepareStyle()
            return
        }
        let getStyle = ((count) => {
            var i = -1
            return () => {
                i += 1
                if (i >= count) i = 0
                return `class="${prefix}${i}"`
            }
        })(prepareStyle())
    
        document.querySelectorAll(selector).forEach((p) => {
            var count = 0
            let proc = makeProcessor()
            var html = `<span ${getStyle()}>`
            let originalHtml = p.innerHTML
            var level = 0
            for (var i = 0; i < originalHtml.length; i++) {
                let c = originalHtml[i]
                level = proc(c)
                if (level === 0 && c !== '>') { count += 1 }
                if (count === countRange) {
                    count = 0
                    html += `</span><span ${getStyle()}>${c}`
                } else { html += c }
            }
            p.innerHTML = html + "</span>"
            if (level > 0) {
                console.log(`level = ${level}`)
                console.log(originalHtml)
            }
        })
    })("p", 60)