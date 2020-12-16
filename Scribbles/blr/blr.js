((selector, countRange) => {
    const debug = true
    const useLinearGradient = false
    const prefix = "prmnsblr"
    const skipElements = new Set(["H1", "H2", "H3", "H4", "H5", "H6", "PRE", "CODE", "SCRIPT", "FORM", "A", "BUTTON", "TH",
        "INPUT", "NAV", "BR", "IMG", "SVG", "DT", "NOSCRIPT", "HR"])
    const whiteSpaces = new Set([" ", "\t", "\r", "\n"])
    function isOnlyWhitespaces(s) {
        for (var i = 0; i < s.length; i++) { if (!whiteSpaces.has(s[i])) return false }
        return true
    }

    function parseAll(path, node, processor) {
        let current = `${path}.${node.nodeName}`
        if (node.nodeType !== 1 /* NODE */) {
            if (debug && node.nodeType !== 8) {
                console.log(`unexpected nodeType: ${node.nodeType}: ${node.nodeName}`)
            }
            return
        }
        let name = node.nodeName.toUpperCase()
        if (!skipElements.has(name) &&
            window.getComputedStyle(node).display !== "none") {
            if (debug) {
                const confirmedList = new Set(["BODY", "DIV", "P", "SPAN", "LI", "UL", "OL", "STRONG", "EM", "I", "B", "NAV", "HEADER", "FOOTER",
                    "TABLE", "TR", "TD", "TBODY", "LABEL", "BIG", "DL", "DD", "CITE", "SUP", "S", "DT", "Q", "BLOCKQUOTE", "SECTION", "U"])
                if (!confirmedList.has(name)) { console.log(`*** ${name} not found in confirmedList.`) }
            }
            node.childNodes.forEach((n) => {
                if (n.nodeType === 1 /* NODE */) { parseAll(current, n, found) }
                else if (n.nodeType === 3 /* #text */) {
                    if (!isOnlyWhitespaces(n.textContent)) { processor.processText(n) }
                }
            })
        }
    }
    function insertStyle(sheet, rule) {
        sheet.insertRule(rule, sheet.cssRules.length)
    }
    function isDupulicateApplication() {
        return (document.querySelector(`.${prefix}0`) !== null)
    }
    function splitColor(color) {
        return color.split(")")[0].split("(")[1].split(", ").map(s => parseInt(s, 10))
    }
    function needsBrighter() {
        var result = false
        let e = document.querySelector(selector)
        if (e !== null) {
            let color = window.getComputedStyle(e).color
            if (color.startsWith("rgb")) {
                let colors = splitColor(color)
                if ((((colors[0] * 299) + (colors[1] * 587) + (colors[2] * 114)) / 1000) > 128) { result = true }
            }
        }
        if (isDupulicateApplication()) { result = !result }
        return result
    }
    function cleanUpOldStyles() {
        Array.from(document.styleSheets).forEach((sheet) => {
            try {
                Array.from(sheet.cssRules).map((v, idx) => [v, idx]).filter((v) => v[0].selectorText !== undefined && v[0].selectorText.startsWith(`span.${prefix}`)).map((v) => v[1]).sort((a, b) => b - a).forEach((v) => {
                    document.styleSheets[0].deleteRule(v)
                })
            } catch (e) { } // https://chromium.googlesource.com/chromium/src/+/a4ebe08c91e29140e700c7bae9b94f27a786d1ca
        })
    }

    var baseColor = "rgb(0, 0, 0)"
    var gradColors = ["rgb(0, 0, 255)", "rgb(255, 0, 0)"]

    if (needsBrighter()) {
        baseColor = "rgb(255, 255, 255)"
        gradColors = ["rgb(24, 255, 0)", "rgb(227, 55, 77)"]
    }

    let processorByLetter = class {
        prepareStyle() {
            var ptr = -1
            function makeStyle(r, g, b) {
                ptr += 1
                let style = `span.${prefix}${ptr} { color: rgb(${r}, ${g}, ${b}); }`
                return style
            }

            let element = document.createElement("style")
            document.head.append(element)
            let sheet = element.sheet
            function grad(v1, v2, f) {
                return parseInt(v1 * (1.0 - f) + v2 * f)
            }
            gradColors.forEach((color) => {
                let rgbFrom = splitColor(baseColor)
                let rgbTo = splitColor(color)
                for (var i = 0.0; i < 1.0; i += 1.0 / countRange) {
                    insertStyle(sheet, makeStyle(
                        grad(rgbFrom[0], rgbTo[0], i),
                        grad(rgbFrom[1], rgbTo[1], i),
                        grad(rgbFrom[2], rgbTo[2], i)
                    ))
                }
            })
            return ptr
        }
        prepareProcess() {
            this.getClass = ((count) => {
                var i = -1
                return () => {
                    i += 1
                    if (i >= count * 2) i = 0
                    let div = parseInt(i / countRange)
                    let rem = i % countRange

                    var idx = 0
                    if (div === 0) { idx = rem }
                    else if (div === 1) { idx = countRange - rem - 1 }
                    else if (div === 2) { idx = countRange + rem }
                    else { idx = 2 * countRange - rem - 1 }
                    return `${prefix}${idx}`
                }
            })(this.prepareStyle())
        }
        processText(node) {
            let root = document.createElement("span")
            for (var i = 0; i < node.textContent.length; i++) {
                let c = node.textContent[i]

                let newChild = document.createElement("span")
                newChild.setAttribute("class", this.getClass())
                newChild.textContent = c
                root.appendChild(newChild)
            }
            node.parentNode.replaceChild(root, node)
        }
    }
    let processorByLinearGradient = class {
        prepareStyle() {
            var ptr = -1
            function makeStyle(c1, c2) {
                ptr += 1
                return `span.${prefix}${ptr} { background: linear-gradient(90deg, ${c1}, ${c2}) !important; -webkit-background-clip: text !important; color: transparent !important; }`
            }

            let element = document.createElement("style")
            document.head.append(element)
            let sheet = element.sheet
            gradColors.forEach((color) => {
                insertStyle(sheet, makeStyle(baseColor, color))
                insertStyle(sheet, makeStyle(color, baseColor))
            })
            return ptr
        }
        prepareProcess() {
            this.getClass = ((count) => {
                var i = -1
                return () => {
                    i += 1
                    if (i > count) i = 0
                    return `${prefix}${i}`
                }
            })(this.prepareStyle())
        }
        processText(node) {
            let root = document.createElement("span")
            let len = node.nodeValue.length
            let text = node.nodeValue

            for (var i = 0; i < len; i += countRange) {
                let newChild = document.createElement("span")
                newChild.setAttribute("class", this.getClass())
                newChild.textContent = text.substring(i, i + countRange)
                root.appendChild(newChild)
            }
            node.parentNode.replaceChild(root, node)
        }
    }

    var processor = useLinearGradient ? (new processorByLinearGradient()) : (new processorByLetter())

    if (isDupulicateApplication()) {
        cleanUpOldStyles()
        processor.prepareStyle()
        return
    }
    processor.prepareProcess()
    parseAll("", document.querySelector(selector), processor)
})("body", 60)
