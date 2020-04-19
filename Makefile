KEY = ""
CER = ""

DEVELOPMENT_OVERRIDE_FILE=docker-compose.development.override.yaml
PRODUCTION_DEFAULT_FILE=docker-compose.production.default.yaml
PRODUCTION_OVERRIDE_FILE=docker-compose.production.override.yaml

development-up:
ifeq (${DEVELOPMENT_OVERRIDE_FILE}, $(shell ls | grep ${DEVELOPMENT_OVERRIDE_FILE}))
	docker-compose -f docker-compose.yaml -f ${DEVELOPMENT_OVERRIDE_FILE} build
	docker-compose -f docker-compose.yaml -f ${DEVELOPMENT_OVERRIDE_FILE} up
else
	docker-compose build
	docker-compose up
endif

production-up:
ifeq (${PRODUCTION_OVERRIDE_FILE}, $(shell ls | grep ${PRODUCTION_OVERRIDE_FILE}))
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_OVERRIDE_FILE} build
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_OVERRIDE_FILE} up -d

else
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_DEFAULT_FILE} build
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_DEFAULT_FILE} up -d
endif

production-http-up:
	docker-compose -f docker-compose.production.http.yaml build
	docker-compose -f docker-compose.production.http.yaml up -d


development-remove:
ifeq (${DEVELOPMENT_OVERRIDE_FILE}, $(shell ls | grep ${DEVELOPMENT_OVERRIDE_FILE}))
	docker-compose -f docker-compose.yaml -f ${DEVELOPMENT_OVERRIDE_FILE} down
	docker-compose -f docker-compose.yaml -f ${DEVELOPMENT_OVERRIDE_FILE} down
else
	docker-compose down
	docker-compose down
endif
	docker volume rm healthreport_db-data healthreport_app-data


development-down:
ifeq (${DEVELOPMENT_OVERRIDE_FILE}, $(shell ls | grep ${DEVELOPMENT_OVERRIDE_FILE}))
	docker-compose -f docker-compose.yaml -f ${DEVELOPMENT_OVERRIDE_FILE} down
	docker-compose -f docker-compose.yaml -f ${DEVELOPMENT_OVERRIDE_FILE} down
else
	docker-compose down
	docker-compose down
endif

production-down:
ifeq (${PRODUCTION_OVERRIDE_FILE}, $(shell ls | grep ${PRODUCTION_OVERRIDE_FILE}))
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_OVERRIDE_FILE} down
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_OVERRIDE_FILE} down
else
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_DEFAULT_FILE} down
	docker-compose -f docker-compose.production.yaml -f ${PRODUCTION_DEFAULT_FILE} down
endif

production-http-down:
	docker-compose -f docker-compose.production.http.yaml down
	docker-compose -f docker-compose.production.http.yaml down




pfx:
	openssl pkcs12 -export -out ./keys/server.pfx -inkey ${KEY} -in ${CER}

clean:
	rm -rf NCVC.App/bin
	rm -rf NCVC.App/obj
	rm -rf NCVC.App/etc
	rm -rf NCVC.App/node_modules
	rm -rf NCVC.App/out/wwwroot
