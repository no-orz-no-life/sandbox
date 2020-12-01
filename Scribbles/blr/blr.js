((selector, countRange) => {
        function makeProcessor() {
            var state = 0
            var level = 0
            return (c) => {
                let beforeState = state
                let beforeLevel = level
                if (state === 0) {
                    if (c === '<') { level += 1; state = 1}
                } else if (state === 1) {
                    if (c === '/')
                        state = 2
    
                    else
                        state = 3
                } else if (state === 2) {
                    if (c === '>') { level -= 2; state = 0}
                } else if (state == 3) {
                    if (c === '>')
                        state = 0
                }
                //console.log(`input: ${c}, level: ${beforeLevel}->${level}, state: ${beforeState}->${state}`)
                return level
            }
        }
    
        let getStyle = (() => {
            var baseColor = "black"
            var gradColors = ["blue", "red"]
    
            // calculate optimal colors
            let e = document.querySelector(selector)
            if (e != null) {
                let color = window.getComputedStyle(e).color
                if (color.startsWith("rgb")) {
                    let colors = color.split(")")[0].split("(")[1].split(", ").map(s => parseInt(s, 10))
                    let r = colors[0]
                    let g = colors[1]
                    let b = colors[2]
                    // TODO: alpha channel
                    if ((((r * 299) + (g * 587) + (b * 114)) / 1000) > 128) {
                        baseColor = "white"
                        gradColors = ["#14ff00", "#e32d4d"]
                    }
                }
            }
    
            let gen = function* () {
                var ptr = 0
                while (1) {
                    yield `${baseColor}, ${gradColors[ptr]}`
                    yield `${gradColors[ptr]}, ${baseColor}`
                    ptr += 1
                    if (ptr >= gradColors.length) {
                        ptr = 0
                    }
                }
            } ()
            return () => `background: linear-gradient(90deg, ${gen.next().value}); -webkit-background-clip: text; -webkit-text-fill-color: transparent;`
        })()
    
        document.querySelectorAll(selector).forEach((p) => {
            var count = 0
            let proc = makeProcessor()
            var html = `<span style="${getStyle()}">`
            let originalHtml = p.innerHTML
            var level = 0
            for (i = 0; i < originalHtml.length; i++) {
                let c = originalHtml[i]
                level = proc(c)
                if (level === 0 && c !== '>')
                    count += 1
                if (count === countRange) {
                    count = 0
                    html += `</span><span style="${getStyle()}">${c}`
                } else {
                    html += c
                }
            }
            p.innerHTML = html + "</span>"
            if (level > 0) {
                console.log(`level = ${level}`)
                console.log(originalHtml)
            }
        })
    })("p", 60)