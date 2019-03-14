XA.component.partialDesignHighlight = (function($,_,document){
    var pub = {}, isRegistered = false;
	
	pub.init = function() {
	    var Frame, mouseoverHandler, mouseoutHandler;

        if(!Object.prototype.hasOwnProperty.call(window, "Sitecore")) {
			return false;
		}

        if ($('.on-page-editor').length > 0 && !isRegistered) {

            isRegistered = true;

            Frame = Sitecore.PageModes.HoverFrame.extend({
                horizontalSideClassName: function() {
                    return this.base() + " scHilghlightedChrome boldHighlight sxaFrame";
                },

                verticalSideClassName: function() {
                    return this.base() + " scHilghlightedChrome boldHighlight sxaFrame";
                },

                dispose: function() {
                    if (this.sides) {
                        $sc.each(this.sides, function() {
                            this.remove();
                        });
                    }
                    this.sides = null;                    
                },
                show: function(chrome) {
                    this.base(chrome);

                    this.top.css('left', this.top.position().left - 2 + "px").css('width', this.top.width() + 4 + "px");
                    this.bottom.css('left', this.bottom.position().left - 2 + "px").css('width', this.bottom.width() + 4 + "px");
                    this.topRightCorner.css('left', this.topRightCorner.position().left + 2 + "px");
                    this.bottomRightCorner.css('top', this.bottomRightCorner.position().top + 3 + "px");
                    this.bottomLeftCorner.css('top', this.bottomLeftCorner.position().top + 3 + "px");
                    this.left.css('height', this.left.height() + 7 + "px").css('top', this.left.position().top - 2 + "px");
                    this.right.css('height', this.right.height() + 7 + "px").css('left', this.right.position().left - 1 + "px").css('top', this.right.position().top - 2 + "px");
                    if (this.bottom.position().top - this.top.position().top < 7) {
                        this.bottom.css('top', this.top.position().top + 7 + "px");
                    }
                }
            });

            mouseoverHandler = function (partialDesignId) {
                
                if (partialDesignId) {
                    var chromes = Sitecore.PageModes.ChromeManager.chromes();
                  
                    for (var i = 0; i < chromes.length; i++) {
                        var chrome = chromes[i];
                        if (chrome.data !== undefined &&
                            chrome.data.custom !== undefined &&
                            partialDesignId === chrome.data.custom.sxaSource) {

                            if (chrome._highlight !== undefined) {
                                chrome._highlight.hide();
                            }
                                
                            chrome._highlight = new Frame();
                            chrome._highlight.show(chrome);
                        }
                    }
                }
            }

            mouseoutHandler = function() {
                var chromes = Sitecore.PageModes.ChromeManager.chromes();
                for (var i = 0; i < chromes.length; i++) {
                    chromes[i].hideHighlight();
                }
                $(".sxaFrame").remove();
            }

            $('.on-page-editor').on('mouseover', 'table[data-zgtype=snippets] tr', function () {
                mouseoverHandler($(this).attr('id'));
			});

			$('.on-page-editor').on('mouseout', 'table[data-zgtype=snippets] tr', mouseoutHandler);

			
			$('.on-page-editor').on('mouseenter', '.sc_DropDownItemLink', function () {
			    
			    //check if we are over snippet button

                var partialDesignId = $(this).data("sc-sxa-item-id");
			    
                if (partialDesignId.length) {
                    partialDesignId = partialDesignId.substring(partialDesignId.indexOf('{') + 1, partialDesignId.indexOf('}'));
                    if (partialDesignId) {
                        mouseoverHandler("L" + partialDesignId.replace(/-/g,''));
                    }
                }
            });

			$('.on-page-editor').on('mouseleave', '.sc_DropDownItemLink', function () {
                //check if we are over snippet button
			    if ($(this).data("sc-sxa-item-id").length > 0) {
                    mouseoutHandler();
                }
            });
		}		
	};
	
    return pub;
})(jQuery,_, document);

XA.register("partialDesignHighlight", XA.component.partialDesignHighlight);