Sitecore.LayoutDefinition = new function() {
};

Sitecore.LayoutDefinition.insert = function(placeholderKey, id) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);

  var r = new Object();
  r["@id"] = id;
  r["@ph"] = placeholderKey;

  device.r.splice(0, 0, r);

  this.setLayoutDefinition(layoutDefinition);
};

Sitecore.LayoutDefinition.getRendering = function(uid) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);
  if (!device) {
    return null;
  }

  for (var n = 0; n < device.r.length; n++) {
    if (this.getShortID(device.r[n]["@uid"]) == uid) {
      return device.r[n];            
    }
  }
};

Sitecore.LayoutDefinition.getRenderings = function () {
  var renderings = [];
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);
  if (!device || !device.r) {
    return renderings;
  }

  for (var n = 0; n < device.r.length; n++) {
    var uid = device.r[n]["@uid"];
    if (uid) {
      device.r[n]["controlId"] = "r_" + this.getShortID(uid);
      renderings.push(device.r[n]);
    }
  }

  return renderings;
};

Sitecore.LayoutDefinition.getRenderingsWithDatasources = function() {
  var renderingsWithDatasources = [];
  var itemId = Sitecore.PageModes.PageEditor.itemID();
  var chromes = Sitecore.PageModes.ChromeManager.chromes();
  var renderings = this.getRenderings();
  $sc.each(renderings, function () {
    var rendering = this;
    $sc.each(chromes, function () {
      var chrome = this;
      if ((rendering["@ds"] && rendering["@ds"] != itemId)
        || (chrome.type.uniqueId && rendering.controlId.indexOf(chrome.type.uniqueId()) != -1 && chrome.data.contextItemUri.indexOf(itemId) == -1)) {
        renderingsWithDatasources.push(rendering);
      }
    });
  });

  return renderingsWithDatasources;
};

Sitecore.LayoutDefinition.remove = function(uid) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);

  this.removeRendering(device, uid);  
  this.setLayoutDefinition(layoutDefinition);
};

Sitecore.LayoutDefinition.removeRendering = function(device, uid) {
  for (n = 0; n < device.r.length; n++) {
    if (this.getShortID(device.r[n]["@uid"]) == uid) {
      var r = device.r[n];
      device.r.splice(n, 1);
      return r;
    }
  }
  return null;
};

Sitecore.LayoutDefinition.moveToPosition = function (uid, placeholderKey, position) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);
  var originalPosition = this._getRenderingPositionInPlaceholder(device, placeholderKey, uid, placeholderKey);
  var r = this.removeRendering(device, uid);
  if (r == null) {
    return;
  }

  Sitecore.LayoutDefinition.handleRelatedRenderings(placeholderKey, r, device.r);

  r["@ph"] = placeholderKey;

  if (position == 0) {
    device.r.splice(0, 0, r);
    this.setLayoutDefinition(layoutDefinition);
    return;
  }
  // Rendering is moving down inside the same placeholder. Decrement the real position, because rendering itself is removed 
  // from his original position. 
  if (originalPosition > -1 && originalPosition < position) {
    position--;
  }

  var placeholderWiseCount = 0;
  for (var totalCount = 0; totalCount < device.r.length; totalCount++) {
    var rendering = device.r[totalCount];
    if (rendering["@ph"] == '') {
      //if @ph value is empty(e.g. when placehoder was set on rendereng but not in ther layout definition)
      //then get placeholder key from DOM
      var identifier = "#r_" + rendering["@uid"].replace(/[{}-]/g, '');
      rendering["@ph"] = $sc(identifier).parent().children().first().attr("key");
    }

    if (Sitecore.PageModes.Utility.areEqualPlaceholders(rendering["@ph"], placeholderKey)) {
      placeholderWiseCount++;
    }

    if (placeholderWiseCount == position)
    {
      device.r.splice(totalCount + 1, 0, r);
      break;
    }
  }

  this.setLayoutDefinition(layoutDefinition);
};

Sitecore.LayoutDefinition.handleRelatedRenderings = function (newPlaceholder, renderingToMove, renderings) {
  if (!renderingToMove) {
    return;
  }

  if (!renderings || renderings.length == 0) {
    return;
  }

  if (newPlaceholder == "") {
    return;
  }

  var chrome = $sc.first(Sitecore.PageModes.ChromeManager.chromes(), function () {
    if (!this.type || typeof this.type.uniqueId != 'function') {
      return false;
    }

    return this.type.uniqueId() === $sc.toShortId(renderingToMove["@uid"]);
  });

  if (!chrome) {
    return;
  }

  var relatedRenderings = Sitecore.LayoutDefinition.getRelatedRenderingsUID(chrome);

  for (var i = 0; i < renderings.length; i++) {
    var r = renderings[i];
    if (relatedRenderings.indexOf($sc.toShortId(r["@uid"])) == -1) {
      continue;
    }

    var renderingPlaceholder = Sitecore.LayoutDefinition.checkPlaceholderPath(r["@ph"]);
    var movedRenderingPlaceholder = Sitecore.LayoutDefinition.checkPlaceholderPath(renderingToMove["@ph"]);

    if (renderingPlaceholder.match("^" + movedRenderingPlaceholder)) {
      r["@ph"] = r["@ph"].replace(movedRenderingPlaceholder, Sitecore.LayoutDefinition.checkPlaceholderPath(newPlaceholder))
    }
  }
};

Sitecore.LayoutDefinition.checkPlaceholderPath = function (placeholder) {
  if (placeholder.indexOf('/') != 0) {
    return '/' + placeholder;
  }

  return placeholder;
};

Sitecore.LayoutDefinition.getRelatedRenderingsUID = function (chrome) {
  var renderingsUid = [];
  var childChromes = chrome.getChildChromes(function () {
    return this.type && typeof this.type.uniqueId == 'function';
  }, true);

  $sc.each(childChromes, function () {
    renderingsUid.push(this.type.uniqueId().toString());
    renderingsUid = renderingsUid.concat(Sitecore.LayoutDefinition.getRelatedRenderingsUID(this));
  });

  return renderingsUid;
};

Sitecore.LayoutDefinition.getRenderingConditions = function(renderingUid) {
  if (!Sitecore.PageModes.Personalization) {
    return [];
  }

  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);
  var conditions = [];
  for (var i = 0; i < device.r.length; i++) {
    if (this.getShortID(device.r[i]["@uid"]) == renderingUid && device.r[i].rls) {
      var rules = device.r[i].rls.ruleset;
      if (rules && rules.rule) {
        if(!$sc.isArray(rules.rule)) {
          rules.rule = [rules.rule];
        }

        for (var j = 0; j < rules.rule.length; j++) {
          var conditionId = rules.rule[j]["@uid"];
          var isActive = Sitecore.PageModes.Personalization.ConditionStateStorage.isConditionActive(renderingUid, conditionId);
          conditions.push(new Sitecore.PageModes.Personalization.Condition(
            conditionId,
            rules.rule[j]["@name"],
            isActive
          ));
        }
      }
    }
  }

  return conditions;
};

Sitecore.LayoutDefinition.GetConditionById = function(conditionId) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);  
  for (var i = 0; i < device.r.length; i++) {
     var rules = device.r[i].rls ? device.r[i].rls.ruleset: null;
     if (rules && rules.rule) {
        if(!$sc.isArray(rules.rule)) {
          rules.rule = [rules.rule];
        }

        for (var j = 0; j < rules.rule.length; j++) {
          if (rules.rule[j]["@uid"] == conditionId) {
            return {rule : rules.rule[j]};
          }
        }
     }
  }

  return {};
};

Sitecore.LayoutDefinition.getRenderingIndex = function(placeholderKey, index) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);

  var i = 0;

  for (n = 0; n < device.r.length; n++) {
    if (device.r[n]["@ph"] == placeholderKey) {
      if (i == index) {
        return n;
      }

      i++;
    }
  }

  return -1;
};

Sitecore.LayoutDefinition.getRenderingPositionInPlaceholder = function(placeholderKey, uid) {
  var layoutDefinition = this.getLayoutDefinition();
  var device = this.getDevice(layoutDefinition);
  return this._getRenderingPositionInPlaceholder(device, placeholderKey, uid);
};

Sitecore.LayoutDefinition.getLayoutDefinition = function() {
  return JSON.parse($sc("#scLayout").val());
};

Sitecore.LayoutDefinition.setLayoutDefinition = function(layoutDefinition) {
  var newValue = $sc.type(layoutDefinition) === "string" ? layoutDefinition : JSON.stringify(layoutDefinition);
  if ($sc("#scLayout").val() != newValue) {
    $sc("#scLayout").val(newValue).change();
    Sitecore.PageModes.PageEditor.setModified(true);
  }
};

Sitecore.LayoutDefinition.getDeviceID = function() {
  return $sc("#scDeviceID").val();
};

Sitecore.LayoutDefinition.getDevice = function(layoutDefinition) {
  var deviceID = this.getDeviceID();

  if (!layoutDefinition.r.d) {
    return null;
  }

  //By serialization behaivour. If there is single element- it would not be serialized as array
  if (!layoutDefinition.r.d.length) {
    layoutDefinition.r.d = [layoutDefinition.r.d];
  }

  var list = layoutDefinition.r.d;

  for (var n = 0; n < list.length; n++) {
    var d = list[n];

    var id = this.getShortID(d["@id"]);

    if (id == deviceID) {
      //By serialization behaivour. If there is single element- it would not be serialized as array
      if (d.r && !d.r.length) {
        d.r = [d.r];
      }
      return d;
    }
  }

  return null;
};

Sitecore.LayoutDefinition.getShortID = function(id) {
  return id.substr(1, 8) + id.substr(10, 4) + id.substr(15, 4) + id.substr(20, 4) + id.substr(25, 12);
};

Sitecore.LayoutDefinition.readLayoutFromRibbon = function() {
  var layout = Sitecore.PageModes.PageEditor.layoutDefinitionControl().value;
  if (layout && layout.length > 0) {
    this.setLayoutDefinition(layout);
    return true;
  }

  return false;
};

Sitecore.LayoutDefinition._getRenderingPositionInPlaceholder = function(device, placeholderKey, uid, defaultPlaceholderKey) {
  var counter = 0;
  for (var i = 0; i < device.r.length; i++) {
    var devicePlaceholder = device.r[i]["@ph"];
    if (devicePlaceholder == '' && defaultPlaceholderKey) {
      devicePlaceholder = defaultPlaceholderKey;
    }

    if (device.r[i]["@ph"] == "" || Sitecore.PageModes.Utility.areEqualPlaceholders(devicePlaceholder, placeholderKey)) {
      if (this.getShortID(device.r[i]["@uid"]) == uid) {
        return counter;
      }

      counter++;
    }
  }

  return -1;
};

