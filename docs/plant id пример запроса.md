curl --location 'https://plant.id/api/v3/identification' \
--header 'Api-Key: aPFHo4BB8ZfXdnWxq6Fj0reMrvVpb6wxc1iaTOZZL0fgJJA02I' \
--header 'Content-Type: application/json' \
--data '{
    "images": ["data:image/jpg;base64,/9j/4AAQSkZJRgABAQEASABIA..."],
    "latitude": 49.207,
    "longitude": 16.608,
    "health" : "all",
    "similar_images": true
}'



Пример ответа
Код 201
Тело ответа:
{
    "access_token": "XFMRsKSGe62ODXd",
    "model_version": "plant_id:5.1.1",
    "custom_id": null,
    "input": {
        "latitude": 49.207,
        "longitude": 16.608,
        "health": "all",
        "similar_images": true,
        "images": [
            "https://plant.id/media/imgs/bac766beb9f5418aac3d919e0ef54d89.jpg"
        ],
        "datetime": "2026-04-23T13:48:32.195343+00:00"
    },
    "result": {
        "disease": {
            "suggestions": [
                {
                    "id": "5ce70d29fa1d9561",
                    "name": "Pucciniales",
                    "probability": 0.0227,
                    "similar_images": [
                        {
                            "id": "948366579a10097851ef86e6743e279f7f110182",
                            "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/948/366579a10097851ef86e6743e279f7f110182.jpeg",
                            "license_name": "CC BY 3.0",
                            "license_url": "https://creativecommons.org/licenses/by/3.0/",
                            "citation": "Johan Adler",
                            "similarity": 0.38,
                            "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/948/366579a10097851ef86e6743e279f7f110182.small.jpeg"
                        },
                        {
                            "id": "2a95e94819e4ea61016afbdc6d956946c994d266",
                            "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/2a9/5e94819e4ea61016afbdc6d956946c994d266.jpg",
                            "license_name": "CC BY 3.0",
                            "license_url": "https://creativecommons.org/licenses/by/3.0/",
                            "citation": "Jared Lincenberg",
                            "similarity": 0.374,
                            "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/2a9/5e94819e4ea61016afbdc6d956946c994d266.small.jpg"
                        }
                    ],
                    "details": {
                        "language": "en",
                        "entity_id": "5ce70d29fa1d9561"
                    }
                }
            ],
            "question": null
        },
        "classification": {
            "suggestions": [
                {
                    "id": "ae8faed4a61d9de2",
                    "name": "Leucojum vernum",
                    "probability": 0.99,
                    "similar_images": [
                        {
                            "id": "5c56885e75c845d7b1ca2034e8f7cdcb2715e8fa",
                            "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/5c5/6885e75c845d7b1ca2034e8f7cdcb2715e8fa.jpg",
                            "similarity": 0.775,
                            "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/5c5/6885e75c845d7b1ca2034e8f7cdcb2715e8fa.small.jpg"
                        },
                        {
                            "id": "0d3a111fcec1035892224e7b3045db73ecfbfd9e",
                            "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/0d3/a111fcec1035892224e7b3045db73ecfbfd9e.jpg",
                            "license_name": "CC BY 4.0",
                            "license_url": "https://creativecommons.org/licenses/by/4.0/",
                            "citation": "Dao Nguyen and James Hardcastle",
                            "similarity": 0.771,
                            "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/0d3/a111fcec1035892224e7b3045db73ecfbfd9e.small.jpg"
                        }
                    ],
                    "details": {
                        "language": "en",
                        "entity_id": "ae8faed4a61d9de2"
                    }
                }
            ]
        },
        "is_plant": {
            "probability": 0.9580986,
            "threshold": 0.5,
            "binary": true
        },
        "is_healthy": {
            "binary": true,
            "probability": 0.9702000286051771,
            "threshold": 0.5
        }
    },
    "status": "COMPLETED",
    "sla_compliant_client": true,
    "sla_compliant_system": true,
    "created": 1776952112.195343,
    "completed": 1776952112.807876
}



ПРИМЕР ОТВЕТА С УТОЧНЯЮЩИМ ВОПРОСОМ

{
  "access_token": "u1UsuKfLbuQ1QZ3",
  "model_version": "plant_id:5.1.1",
  "custom_id": null,
  "input": {
    "latitude": null,
    "longitude": null,
    "health": "all",
    "similar_images": true,
    "images": [
      "https://plant.id/media/imgs/ec3474dba48b478e958dac2838e7df89.jpg"
    ],
    "datetime": "2026-04-28T09:56:58.182148+00:00"
  },
  "result": {
    "classification": {
      "suggestions": [
        {
          "id": "c1396f242d8786ff",
          "name": "Cucumis sativus",
          "probability": 0.99,
          "similar_images": [
            {
              "id": "1c18c4f8e868fdf2c204d68ad71c669ffd263ef0",
              "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/1c1/8c4f8e868fdf2c204d68ad71c669ffd263ef0.jpeg",
              "license_name": "CC BY-NC-SA 4.0",
              "license_url": "https://creativecommons.org/licenses/by-nc-sa/4.0/",
              "citation": "FlowerChecker s.r.o.",
              "similarity": 0.785,
              "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/1c1/8c4f8e868fdf2c204d68ad71c669ffd263ef0.small.jpeg"
            },
            {
              "id": "373d2a60f0a6dd93d9ecc050ffcd1f6f92c7e63f",
              "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/373/d2a60f0a6dd93d9ecc050ffcd1f6f92c7e63f.jpeg",
              "license_name": "CC BY-NC-SA 4.0",
              "license_url": "https://creativecommons.org/licenses/by-nc-sa/4.0/",
              "citation": "FlowerChecker s.r.o.",
              "similarity": 0.757,
              "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/373/d2a60f0a6dd93d9ecc050ffcd1f6f92c7e63f.small.jpeg"
            }
          ],
          "details": {
            "language": "en",
            "entity_id": "c1396f242d8786ff"
          }
        }
      ]
    },
    "is_plant": {
      "probability": 0.9939667,
      "threshold": 0.5,
      "binary": true
    },
    "disease": {
      "suggestions": [
        {
          "id": "edebdcd974b11e08",
          "name": "water deficiency",
          "probability": 0.5394,
          "similar_images": [
            {
              "id": "4643b1183767db7a6ef842d5f22815f6490e9145",
              "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/464/3b1183767db7a6ef842d5f22815f6490e9145.jpg",
              "license_name": "CC BY-NC-SA 4.0",
              "license_url": "https://creativecommons.org/licenses/by-nc-sa/4.0/",
              "citation": "FlowerChecker s.r.o.",
              "similarity": 0.47,
              "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/464/3b1183767db7a6ef842d5f22815f6490e9145.small.jpg"
            },
            {
              "id": "72de35e63153d0e0f7677aba0619faa281dbe859",
              "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/72d/e35e63153d0e0f7677aba0619faa281dbe859.jpg",
              "license_name": "CC BY-NC-SA 4.0",
              "license_url": "https://creativecommons.org/licenses/by-nc-sa/4.0/",
              "citation": "FlowerChecker s.r.o.",
              "similarity": 0.459,
              "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/72d/e35e63153d0e0f7677aba0619faa281dbe859.small.jpg"
            }
          ],
          "details": {
            "language": "en",
            "entity_id": "edebdcd974b11e08"
          }
        },
        {
          "id": "f22fbc7e877b750d",
          "name": "nutrient deficiency",
          "probability": 0.2495,
          "similar_images": [
            {
              "id": "92f3f366fbc861ee52ba620955d01b960cfc06be",
              "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/92f/3f366fbc861ee52ba620955d01b960cfc06be.jpg",
              "license_name": "CC BY-NC-SA 4.0",
              "license_url": "https://creativecommons.org/licenses/by-nc-sa/4.0/",
              "citation": "FlowerChecker s.r.o.",
              "similarity": 0.493,
              "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/92f/3f366fbc861ee52ba620955d01b960cfc06be.small.jpg"
            },
            {
              "id": "0b1b1c15920bcc2f659db740e3174b58795419cb",
              "url": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/0b1/b1c15920bcc2f659db740e3174b58795419cb.jpg",
              "license_name": "CC BY-NC-SA 4.0",
              "license_url": "https://creativecommons.org/licenses/by-nc-sa/4.0/",
              "citation": "FlowerChecker s.r.o.",
              "similarity": 0.467,
              "url_small": "https://plant-id.ams3.cdn.digitaloceanspaces.com/similar_images/5/0b1/b1c15920bcc2f659db740e3174b58795419cb.small.jpg"
            }
          ],
          "details": {
            "language": "en",
            "entity_id": "f22fbc7e877b750d"
          }
        }
      ],
      "question": {
        "text": "Are the leaves soft, limp, or drooping?",
        "translation": "Are the leaves soft, limp, or drooping?",
        "options": {
          "yes": {
            "suggestion_index": 0,
            "entity_id": "edebdcd974b11e08",
            "name": "water deficiency",
            "translation": "Yes"
          },
          "no": {
            "suggestion_index": 1,
            "entity_id": "f22fbc7e877b750d",
            "name": "nutrient deficiency",
            "translation": "No"
          }
        }
      }
    },
    "is_healthy": {
      "binary": false,
      "probability": 0.04270000010728836,
      "threshold": 0.5
    }
  },
  "status": "COMPLETED",
  "sla_compliant_client": true,
  "sla_compliant_system": true,
  "created": 1777370218.182148,
  "completed": 1777370218.788098
}