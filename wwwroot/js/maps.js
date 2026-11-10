// Map helpers built on Leaflet + OpenStreetMap (free, no API key).
// Loaded by Views/Shared/_MapScripts.cshtml on pages that show a map.

// Beirut, used when there is nothing else to center on.
var CD_DEFAULT_CENTER = [33.8938, 35.5018];

var CD_COLORS = { store: '#12a87a', home: '#0099CC', driver: '#f97316' };

function cdCreateMap(elementId, center, zoom) {
    var map = L.map(elementId).setView(center || CD_DEFAULT_CENTER, zoom || 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);
    return map;
}

// A colored dot with a label, e.g. cdMarker(map, 33.89, 35.50, 'store', 'Beirut Burgers').
function cdMarker(map, lat, lng, kind, label) {
    return L.circleMarker([lat, lng], {
        radius: 10, weight: 3, color: '#ffffff',
        fillColor: CD_COLORS[kind] || CD_COLORS.store, fillOpacity: 1
    }).addTo(map).bindTooltip(label);
}

// Map showing a list of points: [{ lat, lng, kind, label }]. Zooms to fit them all.
function cdShowPoints(elementId, points) {
    var valid = points.filter(function (p) { return p.lat != null && p.lng != null; });
    var map = cdCreateMap(elementId, valid.length ? [valid[0].lat, valid[0].lng] : null, 15);
    var markers = valid.map(function (p) { return cdMarker(map, p.lat, p.lng, p.kind, p.label); });
    if (valid.length > 1) {
        map.fitBounds(L.featureGroup(markers).getBounds().pad(0.3));
    }
    return map;
}

// Click-to-pick: clicking the map fills the latitude/longitude inputs.
// If the inputs already have values, the pin starts there.
function cdLocationPicker(elementId, latInputId, lngInputId) {
    var latInput = document.getElementById(latInputId);
    var lngInput = document.getElementById(lngInputId);
    var start = latInput.value && lngInput.value ? [parseFloat(latInput.value), parseFloat(lngInput.value)] : null;

    var map = cdCreateMap(elementId, start, start ? 16 : 12);
    var pin = start ? L.marker(start).addTo(map) : null;

    function setPin(lat, lng) {
        latInput.value = lat.toFixed(6);
        lngInput.value = lng.toFixed(6);
        if (pin) { pin.setLatLng([lat, lng]); } else { pin = L.marker([lat, lng]).addTo(map); }
    }

    map.on('click', function (e) { setPin(e.latlng.lat, e.latlng.lng); });

    // Lets "Use my location" buttons move the pin too.
    return { map: map, setPin: function (lat, lng) { setPin(lat, lng); map.setView([lat, lng], 16); } };
}
