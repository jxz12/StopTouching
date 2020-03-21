mergeInto(LibraryManager.library, {
    OpenNHS: function () {
        document.onmouseup = function()
        {
            window.open("https://www.nhs.uk/live-well/healthy-body/best-way-to-wash-your-hands/");
        	document.onmouseup = null;
        } 
    }
});