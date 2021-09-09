const timeout = 2000
const useTimer = true

const titles = {
    "What’s happening": 4, 
    "Topics to follow": 3,
    "You might like": 3,
    "Who to follow": 3
}

function hideNode(node) {
    node.style.display = "none"
}
function process() {
    const nodes = document.querySelectorAll("h2")
    for(var i = 0; i < nodes.length; i++)
    {
        const node = nodes[i]
        const text = node.textContent
        if(text in titles)
        {
            const l = titles[text]
            var n = node
            for(var j = 0; j < l; j++)
            {
                n = n.parentNode
            }
            hideNode(n)
        }        
    }
}
function callback() {
    process()
    if(useTimer) {
	setTimeout(callback, timeout)
    }
}

document.addEventListener("DOMContentLoaded", callback)

console.log("start Voluntary Censorship")

callback()