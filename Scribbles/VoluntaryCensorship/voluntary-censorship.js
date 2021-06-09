const timeout = 2000
const useTimer = true

function classesToSelector(classes) {
    return "." + classes.replaceAll(" ", ".");
}
function hideBySelector(selector)
{
    const nodes = document.querySelectorAll(selector)
    for(var i = 0; i < nodes.length; i++)
    {
	const node = nodes[i]
	node.style.display = "none"
    }
}
function process() {
    hideBySelector(classesToSelector("css-1dbjc4n r-1ihkh82 r-1in3vh1 r-1867qdf r-1phboty r-rs99b7 r-1ifxtd0 r-1udh08x"))
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




