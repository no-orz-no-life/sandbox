const timeout = 2000
const useTimer = true

const titles = new Set(["What’s happening", "Topics to follow", "You might like", "Who to follow"])

function hideNode(node) {
    node.style.display = "none"
}
function process() {
    const nodes = document.querySelectorAll("h2")
    for(var i = 0; i < nodes.length; i++)
    {
        const node = nodes[i]
        if(titles.has(node.textContent))
        {
            hideNode(node.parentNode.parentNode.parentNode)
            node.style.visibility
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