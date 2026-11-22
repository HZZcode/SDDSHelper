local drawableSprite = require("structs.drawable_sprite")

-- Loenn editor support for the SDDS special golden berry (SDDSHelper/SDDSGoldenBerry).
-- The static preview matches the in-game goldberry art (in-game it is the SpriteBank
-- entry "goldberry": atlas collectables/goldberry/idle00-11 and wings00-9, Center anchor).
local sddsGoldenBerry = {}

sddsGoldenBerry.name = "SDDSHelper/SDDSGoldenBerry"
sddsGoldenBerry.depth = -100

sddsGoldenBerry.fieldInformation = {
    winged = {
        fieldType = "boolean"
    }
}

sddsGoldenBerry.fieldOrder = {
    "x", "y",
    "winged"
}

sddsGoldenBerry.placements = {
    {
        name = "SDDS Golden Berry",
        data = {
            winged = false
        }
    },
    {
        name = "SDDS Golden Berry (Winged)",
        data = {
            winged = true
        }
    }
}

-- Editor preview: the winged variant draws a flap layer behind the berry body,
-- matching the in-game draw order.
function sddsGoldenBerry.sprite(room, entity)
    local sprites = {}

    if entity.winged then
        local wings = drawableSprite.fromTexture("collectables/goldberry/wings00", entity)
        wings:setJustification(0.5, 0.5)
        table.insert(sprites, wings)
    end

    local berry = drawableSprite.fromTexture("collectables/goldberry/idle00", entity)
    berry:setJustification(0.5, 0.5)
    table.insert(sprites, berry)

    return sprites
end

-- Selection box: roughly 16x16, same as the vanilla strawberry's editor box.
function sddsGoldenBerry.select(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local half = 8

    return {
        x - half, y - half + 2,
        x + half, y - half + 2,
        x + half, y + half + 2,
        x - half, y + half + 2
    }
end

return sddsGoldenBerry
