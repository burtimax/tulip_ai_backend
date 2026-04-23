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